using System;
using System.IO;

using AtsPlugin.MotorNoise;
using AtsPlugin.Processing;

namespace AtsPlugin
{
    class DmEngine : IAtsBehaviour
    {
        private static DmDebugForm DebugForm = null;

        public class FuelController
        {
            public static double[] FuelTable { get; set; } = null;
            public const double FuelValueOnCutOff = 0.19;

            private readonly double MinimumFuelCurrent = 0.01;
            private readonly double TransitionDuration = 0.0;
            private readonly double TransitonDurationForCutOff = 1500.0;
            private double CurrentTime { get; set; } = 0.0;
            private double CurrentTimeForCutOff { get; set; } = 0.0;
            private int CurrentNotch { get; set; } = 0;
            private int LastNotch { get; set; } = 0;
            private AtsOperationRationalDelay Jerk { get; set; } = new AtsOperationRationalDelay();
            private bool IsCutOffFuel { get; set; } = false;

            public int InNotch { get; set; } = 0;
            public double InRpm { get; set; } = 0.0;
            public double InFuelInjectionCurrent { get; set; } = 0.0;
            public double OutFuelCurrent { get; private set; } = 0.0;


            public void Update(double deltaTime)
            {
                if (CurrentNotch != InNotch)
                {
                    CurrentTime = 0.0;
                    CurrentNotch = InNotch;
                }


                if (CurrentTime < TransitionDuration)
                {
                    OutFuelCurrent = MinimumFuelCurrent;
                    CurrentTime += deltaTime;

                    return;
                }


                var notch = Math.Max(Math.Min(CurrentNotch, FuelTable.Length), 0);


                if ((LastNotch > 0) && (CurrentNotch == 0))
                {
                    IsCutOffFuel = true;
                }
                else if (CurrentNotch > 0)
                {
                    IsCutOffFuel = false;
                }


                if (IsCutOffFuel)
                {
                    if (InRpm >= FuelTable[0] * MaximumRpm)
                    {
                        CurrentTimeForCutOff = 0.0;
                    }

                    if (CurrentTimeForCutOff < TransitonDurationForCutOff)
                    {
                        CurrentTimeForCutOff += deltaTime;
                    }
                    else
                    {
                        IsCutOffFuel = false;
                    }
                }


                if (InFuelInjectionCurrent == 0.0)
                {
                    Jerk.U = IsCutOffFuel ? FuelValueOnCutOff : FuelTable[notch];

                    if (CurrentNotch > LastNotch)
                    {
                        Jerk.Tp = ((LastNotch == 0) && (CurrentNotch == 1)) ? 1.0 : 9000.0;
                    }
                    else if (CurrentNotch < LastNotch)
                    {
                        Jerk.Tp = 3500.0;
                    }
                }
                else
                {
                    Jerk.U = InFuelInjectionCurrent;
                    Jerk.Tp = 1000.0;
                }

                Jerk.Calculate(deltaTime);
                

                OutFuelCurrent = Jerk.Y;

                LastNotch = CurrentNotch;
            }
        }

        public class Governor
        {
            private readonly double MinimumGain = 0.9;
            private readonly double MaximumGain = 1.1;
            
            public double InTargetRpm { get; set; } = 0.0;
            public double InActualRpm { get; set; } = 0.0;
            public double OutGain { get; private set; } = 0.0;


            public void Update()
            {
                if (InActualRpm <= 0.0)
                {
                    OutGain = MinimumGain;
                    return;
                }

                if (InActualRpm >= MaximumRpm)
                {
                    OutGain = 1.0;
                }
                else
                {
                    var ratio = (InTargetRpm - InActualRpm) / MaximumRpm + 1.0;
                    OutGain = Math.Max(Math.Min(ratio, MaximumGain), MinimumGain);
                }


                DebugForm.SetText(DebugForm.GovernerRatio, OutGain.ToString("F3"));
                DebugForm.SetText(DebugForm.ActualRpm, InActualRpm.ToString("0000"));
                DebugForm.SetText(DebugForm.TargetRpm, InTargetRpm.ToString("0000"));
            }
        }

        public class Transmission
        {
            public class TorqueConverter
            {
                private readonly double Tp = 100.0;

                private AtsOperationDelay TorqueConverterDelay { get; set; } = new AtsOperationDelay();

                public double InVelocity { get; set; } = 0.0;
                public double InRpm { get; set; } = 0.0;
                public double OutRpm { get; private set; } = 0.0;


                public void Reset(double value)
                {
                    InRpm = value;
                    TorqueConverterDelay.Reset(value);
                }

                public void Update(double deltaTime)
                {
                    TorqueConverterDelay.U = InRpm;
                    TorqueConverterDelay.Tp = Tp;
                    TorqueConverterDelay.Calculate(deltaTime);

                    OutRpm = TorqueConverterDelay.Y + ((InVelocity / Transmission.TransitionVelocityPositive[1]) * 100.0);
                }
            }

            public class Clutch
            {
                public const double DefaultEngagementTransitionDuration = 500.0;
                public const double DefaultDisengagementTransitionDuration = 0.0;
                private double CurrentTime { get; set; } = 0.0;
                private double CurrentTransitionDuration { get; set; } = 0.0;

                public double EngagementTransitionDuration { private set; get; }
                public double DisengagementTransitionDuration { private get; set; }
                public bool InEngaged { get; set; } = false;
                public bool OutEngaged { get; private set; } = false;


                public Clutch(double engagementTransitionDuration = DefaultEngagementTransitionDuration, double disengagementTransitionDuration = DefaultDisengagementTransitionDuration)
                {
                    EngagementTransitionDuration = engagementTransitionDuration;
                    DisengagementTransitionDuration = disengagementTransitionDuration;
                }

                public void Update(double deltaTime)
                {
                    if (InEngaged == OutEngaged)
                    {
                        CurrentTime = 0.0;
                        CurrentTransitionDuration = 0.0;

                        return;
                    }


                    if (InEngaged)
                    {
                        CurrentTransitionDuration = EngagementTransitionDuration;
                    }
                    else
                    {
                        CurrentTransitionDuration = DisengagementTransitionDuration;
                    }


                    if (CurrentTime < CurrentTransitionDuration)
                    {
                        CurrentTime += deltaTime;

                        return;
                    }


                    OutEngaged = InEngaged;
                    CurrentTime = 0.0;
                }
            }

            public enum DrivingState
            {
                Neutral,
                Synching,
                Drive,
                Brake
            }

            public enum SynchingState
            {
                WaitOperation,
                DisengageAllGearClutch,
                WaitDisengagingAllGearClutch,
                EngageMissionClutch,
                WaitEngagingMissionClutch,
                Sync,
                WaitSyncing,
                EngageGearClutchForHensoku,
                WaitEngagingGearClutchForHensoku,
                EngageGearClutch,
                WaitEngagingGearClutch,
                Stabilize
            }

            public enum GearPositionState
            {
                Neutral = 0,
                Hensoku = 1,
                Mission1,
                Mission2
            }

            public int[][] TractionPositionTable { get; set; } = null;
            public static double[] TransitionVelocityPositive { get; set; } = null;
            public static double[] TransitionVelocityNegative { get; set; } = null;
            public static double EngineBrakeEndVelocity { get; set; } = 0.0;
            public static int TransmissionSyncingStabilizeNotch { get; set; } = 0;
            public static int TransmissionSyncingReductionConditionNotch { get; set; } = 0;
            public static int TransmissionSyncingReductionNotch { get; set; } = 0;
            public static double EngineBlowUpRpm { get; set; } = 0.0;

            private static readonly double DefaultEngineDelayTp = 1600.0;

            private TorqueConverter Converter { get; set; } = new TorqueConverter();
            private double CurrentTime { get; set; } = 0.0;
            private Random BacklashRand { get; set; } = new Random();
            private AtsOperationDelay BacklashDelay { get; set; } = new AtsOperationDelay();
            private Clutch MissionClutch { get; set; } = new Clutch(0.0, 1000.0);
            private Clutch Gear1Clutch { get; set; } = new Clutch(1000.0, 0.0);
            private Clutch Gear2Clutch { get; set; } = new Clutch(1000.0, 0.0);
            private Clutch ForwardClutch { get; set; } = new Clutch(1500.0, 1500.0);
            private Clutch BackwardClutch { get; set; } = new Clutch(1500.0, 1500.0);
            private bool IsEnabledEngineBrake { get; set; } = false;
            private int CurrentReverserPosition { get; set; } = 0;

            public SynchingState CcsSyncingState { get; private set; } = SynchingState.WaitOperation;
            public DrivingState OrderedDrivingState { get; private set; } = DrivingState.Neutral;
            public DrivingState CurrentDrivingState { get; private set; } = DrivingState.Neutral;
            public GearPositionState OrderedGearPosition { get; private set; } = GearPositionState.Neutral;
            public GearPositionState CurrentGearPosition { get; private set; } = GearPositionState.Neutral;
            

            public int InNotch { get; set; } = 0;
            public bool InEngineBrake { get; set; } = false;
            public int InReverser { get; set; } = 0;
            public double InRpm { get; set; } = 0.0;

            public double OutRpm { get; private set; } = 0.0;
            public int OutNotch { get; private set; } = 0;
            public double OutFuelInjectionCurrent { get; private set; } = 0.0;
            public int OutReverserPosition { get; private set; } = 0;
            public int OutTractionPosition { get; private set; } = 0;
            public double OutTransferGearNoiseVolume { get; private set; } = 0.0;
            public double OutEngineDelayTp { get; private set; } = DefaultEngineDelayTp;


            public void Update(double deltaTime)
            {
                var absoluteVelocity = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;


                OutEngineDelayTp = DefaultEngineDelayTp;


                BacklashDelay.U = 1.0f - BacklashRand.Next(0, Math.Max(100 - (int)(AtsSimulationEnvironment.Instance.CurrentStates.MainCircuitCurrent * 2.0), 0)) / 100.0f;
                BacklashDelay.Tp = 70.0;
                BacklashDelay.Calculate(AtsSimulationEnvironment.Instance.DeltaTime);


                if (OutRpm >= EngineBlowUpRpm)
                {
                    // Engine blow up.
                    InNotch = 0;
                }


                if (absoluteVelocity < 5.0f)
                {
                    CurrentReverserPosition = InReverser;
                }


                if (absoluteVelocity <= EngineBrakeEndVelocity)
                {
                    IsEnabledEngineBrake = false;
                }
                else
                {
                    IsEnabledEngineBrake = true;
                }


                switch (InNotch)
                {
                    case 0:
                        if (InEngineBrake && IsEnabledEngineBrake)
                        {
                            OrderedDrivingState = DrivingState.Brake;
                            break;
                        }

                        OrderedDrivingState = DrivingState.Neutral;
                        break;
                    default:
                        if (OrderedDrivingState != DrivingState.Neutral)
                        {
                            break;
                        }


                        OrderedDrivingState = DrivingState.Drive;
                        break;
                }

                
                switch (CurrentReverserPosition)
                {
                    case 0:
                        ForwardClutch.InEngaged = false;
                        BackwardClutch.InEngaged = false;
                        break;
                    case 1:
                        if (!BackwardClutch.OutEngaged)
                        {
                            ForwardClutch.InEngaged = true;
                        }

                        BackwardClutch.InEngaged = false;
                        break;
                    case -1:
                        ForwardClutch.InEngaged = false;

                        if (!ForwardClutch.OutEngaged)
                        {
                            BackwardClutch.InEngaged = true;
                        }
                        break;
                }


                ForwardClutch.Update(deltaTime);
                BackwardClutch.Update(deltaTime);


                if (!ForwardClutch.OutEngaged && !BackwardClutch.OutEngaged)
                {
                    OrderedDrivingState = DrivingState.Neutral;
                    AtsSimulationEnvironment.Instance.PanelOperations["FwLp"] = 0;
                    AtsSimulationEnvironment.Instance.PanelOperations["BwLp"] = 0;
                }
                else if (ForwardClutch.OutEngaged)
                {
                    AtsSimulationEnvironment.Instance.PanelOperations["FwLp"] = 1;
                    AtsSimulationEnvironment.Instance.PanelOperations["BwLp"] = 0;
                }
                else if (BackwardClutch.OutEngaged)
                {
                    AtsSimulationEnvironment.Instance.PanelOperations["FwLp"] = 0;
                    AtsSimulationEnvironment.Instance.PanelOperations["BwLp"] = 1;
                }
                

                switch (OrderedDrivingState)
                {
                    case DrivingState.Neutral:

                        CurrentDrivingState = DrivingState.Neutral;

                        break;
                    case DrivingState.Drive:
                        if (CurrentDrivingState == DrivingState.Neutral)
                        {
                            if (absoluteVelocity < TransitionVelocityPositive[(int)GearPositionState.Hensoku])
                            {
                                OrderedGearPosition = GearPositionState.Hensoku;
                            }
                            else if (absoluteVelocity < TransitionVelocityPositive[(int)GearPositionState.Mission1])
                            {
                                OrderedGearPosition = GearPositionState.Mission1;
                            }
                            else
                            {
                                OrderedGearPosition = GearPositionState.Mission2;
                            }

                            CcsSyncingState = SynchingState.WaitOperation;
                            CurrentDrivingState = DrivingState.Synching;
                        }

                        break;
                    case DrivingState.Brake:
                        if (CurrentDrivingState == DrivingState.Neutral)
                        {
                            CcsSyncingState = SynchingState.WaitOperation;
                            CurrentDrivingState = DrivingState.Synching;
                            if (absoluteVelocity < TransitionVelocityNegative[(int)GearPositionState.Mission1])
                            {
                                OrderedGearPosition = GearPositionState.Mission1;
                            }
                            else
                            {
                                OrderedGearPosition = GearPositionState.Mission2;
                            }
                        }

                        break;
                }


                var notch = InNotch;
                var gear1Rpm = absoluteVelocity / TransitionVelocityPositive[(int)GearPositionState.Mission1] * MaximumGear1Rpm;;
                var gear2Rpm = absoluteVelocity / MaximumGear2Velocity * MaximumGear2Rpm;
                var gear1TargetFuelCurrent = absoluteVelocity / TransitionVelocityPositive[(int)GearPositionState.Mission1];
                var gear2TargetFuelCurrent = absoluteVelocity / MaximumGear2Velocity;

                // CCS
                switch (CurrentDrivingState)
                {
                    case DrivingState.Neutral:
                        Gear1Clutch.InEngaged = false;
                        Gear2Clutch.InEngaged = false;

                        OutNotch = notch;
                        OutFuelInjectionCurrent = 0.0;
                        break;

                    case DrivingState.Synching:
                        OutNotch = 0;
                        OutFuelInjectionCurrent = 0.0;

                        switch (CcsSyncingState)
                        {
                            case SynchingState.WaitOperation:
                                CcsSyncingState = SynchingState.DisengageAllGearClutch;
                                break;

                            case SynchingState.DisengageAllGearClutch:
                                Gear1Clutch.InEngaged = false;
                                Gear2Clutch.InEngaged = false;

                                CcsSyncingState = SynchingState.WaitDisengagingAllGearClutch;
                                break;

                            case SynchingState.WaitDisengagingAllGearClutch:
                                Gear1Clutch.InEngaged = false;
                                Gear2Clutch.InEngaged = false;

                                if (Gear1Clutch.OutEngaged || Gear2Clutch.OutEngaged)
                                {
                                    break;
                                }

                                CcsSyncingState = SynchingState.EngageMissionClutch;
                                break;

                            case SynchingState.EngageMissionClutch:
                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Hensoku:
                                        MissionClutch.InEngaged = false;
                                        break;

                                    case GearPositionState.Mission1:
                                    case GearPositionState.Mission2:
                                        MissionClutch.InEngaged = true;
                                        
                                        break;
                                }

                                CcsSyncingState = SynchingState.WaitEngagingMissionClutch;
                                break;

                            case SynchingState.WaitEngagingMissionClutch:
                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Hensoku:
                                        MissionClutch.InEngaged = false;
                                        break;

                                    case GearPositionState.Mission1:
                                    case GearPositionState.Mission2:
                                        MissionClutch.InEngaged = true;

                                        break;
                                }

                                if (MissionClutch.InEngaged != MissionClutch.OutEngaged)
                                {
                                    break;
                                }

                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Mission1:
                                    case GearPositionState.Mission2:
                                        break;

                                    default:
                                        break;
                                }
                                

                                CcsSyncingState = SynchingState.Sync;
                                break;

                            case SynchingState.Sync:
                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Hensoku:
                                        CcsSyncingState = SynchingState.EngageGearClutchForHensoku;
                                        break;
                                    case GearPositionState.Mission1:
                                        OutFuelInjectionCurrent = gear1TargetFuelCurrent;


                                        if (InRpm < gear1Rpm - 100.0)
                                        {
                                            break;
                                        }

                                        CcsSyncingState = SynchingState.WaitSyncing;

                                        break;

                                    case GearPositionState.Mission2:
                                        OutFuelInjectionCurrent = gear2TargetFuelCurrent;


                                        if (InRpm < gear2Rpm - 100.0)
                                        {
                                            break;
                                        }

                                        CcsSyncingState = SynchingState.WaitSyncing;

                                        break;
                                }
                                
                                break;

                            case SynchingState.WaitSyncing:
                                var diffRpm = 0.0;

                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Mission1:
                                        diffRpm = InRpm - gear1Rpm;

                                        break;
                                    case GearPositionState.Mission2:
                                        diffRpm = InRpm - gear2Rpm;

                                        break;
                                }


                                if (diffRpm >= 100.0)
                                {
                                    break;
                                }


                                CurrentTime = 0.0;
                                CcsSyncingState = SynchingState.EngageGearClutch;
                                break;

                            case SynchingState.EngageGearClutchForHensoku:
                                Gear1Clutch.InEngaged = true;

                                CcsSyncingState = SynchingState.WaitEngagingGearClutchForHensoku;
                                break;

                            case SynchingState.WaitEngagingGearClutchForHensoku:
                                Gear1Clutch.InEngaged = true;

                                if (!Gear1Clutch.OutEngaged)
                                {
                                    break;
                                }
                                
                                CurrentDrivingState = DrivingState.Drive;
                                CcsSyncingState = SynchingState.WaitOperation;
                                break;

                            case SynchingState.EngageGearClutch:
                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Hensoku:
                                    case GearPositionState.Mission1:
                                        Gear1Clutch.InEngaged = true;
                                        Gear2Clutch.InEngaged = false;

                                        break;

                                    case GearPositionState.Mission2:
                                        Gear1Clutch.InEngaged = false;
                                        Gear2Clutch.InEngaged = true;

                                        break;
                                }

                                CcsSyncingState = SynchingState.WaitEngagingGearClutch;
                                break;

                            case SynchingState.WaitEngagingGearClutch:
                                CurrentTime += deltaTime;

                                switch (OrderedGearPosition)
                                {
                                    case GearPositionState.Hensoku:
                                    case GearPositionState.Mission1:
                                        Gear1Clutch.InEngaged = true;
                                        Gear2Clutch.InEngaged = false;

                                        break;

                                    case GearPositionState.Mission2:
                                        Gear1Clutch.InEngaged = false;
                                        Gear2Clutch.InEngaged = true;

                                        break;
                                }

                                if (CurrentTime > 500.0)
                                {
                                    OutNotch = TransmissionSyncingStabilizeNotch;
                                }


                                if (!Gear1Clutch.OutEngaged && !Gear2Clutch.OutEngaged)
                                {
                                    break;
                                }

                                
                                CurrentTime = 0.0;
                                CcsSyncingState = SynchingState.Stabilize;
                                break;

                            case SynchingState.Stabilize:
                                CurrentTime += deltaTime;


                                if (OrderedDrivingState == DrivingState.Brake)
                                {
                                    CcsSyncingState = SynchingState.WaitOperation;
                                    CurrentDrivingState = DrivingState.Brake;
                                    break;
                                }


                                if (CurrentTime > 300.0)
                                {
                                    OutNotch = TransmissionSyncingStabilizeNotch;


                                    if (InNotch >= TransmissionSyncingReductionConditionNotch)
                                    {
                                        if (CurrentTime < 1500.0)
                                        {
                                            OutNotch = TransmissionSyncingReductionNotch;
                                        }
                                        else
                                        {
                                            CcsSyncingState = SynchingState.WaitOperation;
                                            CurrentDrivingState = DrivingState.Drive;

                                            OutNotch = InNotch;
                                        }
                                    }
                                    else
                                    {
                                        CcsSyncingState = SynchingState.WaitOperation;
                                        CurrentDrivingState = DrivingState.Drive;

                                        OutNotch = InNotch;
                                    }
                                }


                                break;
                        }
                        
                        break;

                    case DrivingState.Drive:
                        var lastGearPosition = OrderedGearPosition;


                        switch (CurrentGearPosition)
                        {
                            case GearPositionState.Hensoku:
                                if (absoluteVelocity >= TransitionVelocityPositive[(int)GearPositionState.Hensoku])
                                {
                                    OrderedGearPosition = GearPositionState.Mission1;
                                }
                                break;
                            case GearPositionState.Mission1:
                                if (absoluteVelocity >= TransitionVelocityPositive[(int)GearPositionState.Mission1])
                                {
                                    OrderedGearPosition = GearPositionState.Mission2;
                                }
                                else if (absoluteVelocity <= TransitionVelocityNegative[(int)GearPositionState.Mission1])
                                {
                                    OrderedGearPosition = GearPositionState.Hensoku;
                                }
                                break;
                            case GearPositionState.Mission2:
                                if (absoluteVelocity <= TransitionVelocityNegative[(int)GearPositionState.Mission2])
                                {
                                    OrderedGearPosition = GearPositionState.Mission1;
                                }
                                break;
                        }


                        if (lastGearPosition != OrderedGearPosition)
                        {
                            CurrentDrivingState = DrivingState.Synching;
                        }


                        OutNotch = notch;
                        OutFuelInjectionCurrent = 0.0;
                        break;


                    case DrivingState.Brake:
                        OutFuelInjectionCurrent = 0.0;
                        break;
                }


                if ((ForwardClutch.InEngaged && (ForwardClutch.InEngaged != ForwardClutch.OutEngaged)) || (BackwardClutch.InEngaged && (BackwardClutch.InEngaged != BackwardClutch.OutEngaged)))
                {
                    OutFuelInjectionCurrent = FuelController.FuelValueOnCutOff + 0.03;
                }


                MissionClutch.Update(deltaTime);
                Gear1Clutch.Update(deltaTime);
                Gear2Clutch.Update(deltaTime);


                if (!MissionClutch.OutEngaged && Gear1Clutch.OutEngaged)
                {
                    CurrentGearPosition = GearPositionState.Hensoku;
                }
                else if (Gear1Clutch.OutEngaged)
                {
                    CurrentGearPosition = GearPositionState.Mission1;
                }
                else if (Gear2Clutch.OutEngaged)
                {
                    CurrentGearPosition = GearPositionState.Mission2;
                }
                else
                {
                    CurrentGearPosition = GearPositionState.Neutral;
                    OutNotch *= 2;
                }


                OutReverserPosition = CurrentReverserPosition;


                switch (CurrentDrivingState)
                {
                    case DrivingState.Neutral:
                    case DrivingState.Synching:
                    case DrivingState.Drive:
                        OutNotch = Math.Min(OutNotch, AtsSimulationEnvironment.Instance.ControlHandle.MaximumTractionPosition);
                        OutTractionPosition = TractionPositionTable[(int)CurrentGearPosition][OutNotch];

                        break;
                    case DrivingState.Brake:
                        OutNotch = 0;
                        OutTractionPosition = TractionPositionTable[(int)CurrentGearPosition][9]; ;

                        break;
                }


                switch (CurrentGearPosition)
                {
                    case GearPositionState.Neutral:
                        OutRpm = InRpm;
                        OutTransferGearNoiseVolume = 0.0;
                        Converter.InVelocity = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;
                        Converter.Reset(InRpm * 0.9);
                        Converter.Update(deltaTime);
                        break;

                    case GearPositionState.Hensoku:
                        Converter.InVelocity = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;
                        Converter.InRpm = InRpm;
                        Converter.Update(deltaTime);
                        OutRpm = Converter.OutRpm;

                        if (InRpm < FuelController.FuelTable[1] * MaximumRpm)
                        {
                            OutEngineDelayTp = 500.0;
                        }
                        
                        break;

                    case GearPositionState.Mission1:
                        OutRpm = gear1Rpm;
                        OutTransferGearNoiseVolume = 1.0f * (float)BacklashDelay.Y;
                        Converter.InVelocity = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;
                        Converter.Reset(OutRpm);
                        break;

                    case GearPositionState.Mission2:
                        OutRpm = gear2Rpm;
                        OutTransferGearNoiseVolume = 1.0f * (float)BacklashDelay.Y;
                        Converter.InVelocity = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;
                        Converter.Reset(OutRpm);
                        break;
                }


                AtsSimulationEnvironment.Instance.PanelOperations["ExBLp"] = CurrentDrivingState == DrivingState.Brake ? 1 : 0;

                DebugForm.SetText(DebugForm.MissionClutch,
                    MissionClutch.OutEngaged ? "直結" : "変速");
                DebugForm.SetText(DebugForm.Gear1Clutch,
                    Gear1Clutch.OutEngaged ? "ON" : "OFF");
                DebugForm.SetText(DebugForm.Gear2Clutch,
                    Gear2Clutch.OutEngaged ? "ON" : "OFF");
                DebugForm.SetText(DebugForm.ForwardClutch,
                    ForwardClutch.OutEngaged ? "ON" : "OFF");
                DebugForm.SetText(DebugForm.BackwardClutch,
                    BackwardClutch.OutEngaged ? "ON" : "OFF");
                DebugForm.SetText(DebugForm.CurrentDrivingState,
                    CurrentDrivingState.ToString());
                DebugForm.SetText(DebugForm.CcsSyncingState,
                    CcsSyncingState.ToString());
            }
        }

        public static double MaximumRpm { get; private set; } = 0.0;
        public static double MaximumGear1Rpm { get; private set; } = 0.0;
        public static double MaximumGear2Rpm { get; private set; } = 0.0;
        public static double MaximumGear2Velocity { get; private set; } = 0.0;
        public static int[] EngineBrakeEnableBrakeNotches { get; private set; } = null;

        private AtsMotorNoise EngineNoise { get; set; } = null;
        private AtsMotorNoise GearNoise { get; set; } = null;
        private AtsMotorNoise TransferGear1Noise { get; set; } = null;
        private AtsMotorNoise TransferGear2Noise { get; set; } = null;
        private AtsMotorNoise ExhaustNoise { get; set; } = null;
        private AtsMotorNoise TurbineNoise { get; set; } = null;
        private AtsOperationDelay EngineDelay { get; set; } = new AtsOperationDelay();
        private AtsOperationDelay TurbineDelay { get; set; } = new AtsOperationDelay();
        private FuelController FuelControl { get; set; } = new FuelController();
        private Governor ElectricGovernor { get; set; } = new Governor();
        private Transmission Tacn { get; set; } = new Transmission();


        public void Awake(AtsSimulationEnvironment environment)
        {
            DebugForm = new DmDebugForm();
            AtsMotorNoise.Startup();
            AtsSimulationEnvironment.Instance.MaximumDeltaTime = (1.0 / 15.0) * 1000.0;

            var moduleAddress = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString("SoundFilesRootPath");
            if (!Path.IsPathRooted(moduleAddress))
            {
                moduleAddress = Path.Combine(AtsModule.ModuleDirectoryPath, moduleAddress);
            }



            EngineNoise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(EngineNoise) +  "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(EngineNoise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(EngineNoise) + "SoundTxtSectionName"));
            EngineNoise.Volume = 1.0f;


            GearNoise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(GearNoise) + "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(GearNoise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(GearNoise) + "SoundTxtSectionName"));
            GearNoise.Volume = 0.0f;


            TransferGear1Noise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear1Noise) +  "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear1Noise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear1Noise) + "SoundTxtSectionName"));
            TransferGear1Noise.Volume = 0.0f;


            TransferGear2Noise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear2Noise) + "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear2Noise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TransferGear2Noise) + "SoundTxtSectionName"));
            TransferGear2Noise.Volume = 0.0f;


            ExhaustNoise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(ExhaustNoise) + "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(ExhaustNoise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(ExhaustNoise) + "SoundTxtSectionName"));
            ExhaustNoise.Volume = 0.0f;


            TurbineNoise = AtsMotorNoiseImporter.LoadAsset(Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TurbineNoise) + "MotorNoiseTxtFileName")), Path.Combine(moduleAddress, AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TurbineNoise) + "SoundTxtFileName")), AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsString(nameof(TurbineNoise) + "SoundTxtSectionName"));
            TurbineNoise.Volume = 1.0f;

            AtsSimulationEnvironment.Instance.PanelOperations.Add("FwLp", 8);
            AtsSimulationEnvironment.Instance.PanelOperations.Add("BwLp", 9);
            AtsSimulationEnvironment.Instance.PanelOperations.Add("ExBLp", 18);


            FuelController.FuelTable = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64Array("FuelTable");

            var notchIdle = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32Array("NotchIdle");
            var notchHensoku = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32Array("NotchHensoku");
            var notchMission1 = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32Array("NotchMission1");
            var notchMission2 = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32Array("NotchMission2");

            Tacn.TractionPositionTable = new[] {notchIdle, notchHensoku, notchMission1, notchMission2 };

            Transmission.TransitionVelocityPositive =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64Array("TransitionVelocityPositive");
            Transmission.TransitionVelocityNegative =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64Array("TransitionVelocityNegative");

            Transmission.EngineBrakeEndVelocity =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("EngineBrakeEndVelocity");
            Transmission.TransmissionSyncingStabilizeNotch =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32("TransmissionSyncingStabilizeNotch");
            Transmission.TransmissionSyncingReductionConditionNotch =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32("TransmissionSyncingReductionConditionNotch");
            Transmission.TransmissionSyncingReductionNotch =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32("TransmissionSyncingReductionNotch");
            Transmission.EngineBlowUpRpm = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("EngineBlowUpRpm");



            MaximumRpm = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("MaximumRpm");
            MaximumGear1Rpm = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("MaximumGear1Rpm");
            MaximumGear2Rpm = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("MaximumGear2Rpm");
            MaximumGear2Velocity = AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsFp64("MaximumGear2Velocity");
            EngineBrakeEnableBrakeNotches =
                AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsInt32Array("EngineBrakeEnableBrakeNotches");


            if (AtsSimulationEnvironment.Instance.PluginParameters.GetParameterAsBoolean("ShowDebugDialog"))
            {
                DebugForm.Show();
            }
        }

        public void OnActivate()
        {
        }

        public void OnBeaconDataReceived(AtsBeaconData beaconData)
        {
        }

        public void OnControlHandleMoved(int position, AtsSimulationEnvironment.ControlHandleType controlHandleType)
        {
        }

        public void OnDestroy()
        {
            DebugForm.Close();
            AtsMotorNoise.Cleanup();
        }

        public void OnDoorStateChanged(AtsSimulationStates.DoorStateType doorState)
        {
        }

        public void OnHornBlew(AtsHornType hornType)
        {
        }

        public void OnSignalChanged(int signalIndex)
        {
        }
        

        public void OnStart(AtsInitialHandlePosition initialHandlePosition)
        {
        }

        public void Update()
        {

            Tacn.InEngineBrake = false;

            foreach (var brakePosition in EngineBrakeEnableBrakeNotches)
            {
                if (brakePosition == AtsSimulationEnvironment.Instance.ControlHandle.BrakePosition)
                {
                    Tacn.InEngineBrake = true;
                    break;
                }
            }


            if (AtsSimulationEnvironment.Instance.ControlHandle.IsAppliedEmergencyBrake)
            {
                AtsSimulationEnvironment.Instance.ControlHandle.TractionPosition = 0;
            }

            Tacn.InNotch = AtsSimulationEnvironment.Instance.ControlHandle.TractionPosition;
            Tacn.InReverser = AtsSimulationEnvironment.Instance.ControlHandle.ReverserPosition;

            FuelControl.InRpm = Tacn.OutRpm;
            FuelControl.InNotch = Tacn.OutNotch;
            FuelControl.InFuelInjectionCurrent = Tacn.OutFuelInjectionCurrent;
            FuelControl.Update(AtsSimulationEnvironment.Instance.DeltaTime);

            var injectionCurrentGain = ElectricGovernor.OutGain;
            var fuelInjectionCurrent = FuelControl.OutFuelCurrent * injectionCurrentGain;
            var targetRpm = fuelInjectionCurrent * MaximumRpm;

            EngineDelay.U = targetRpm;
            EngineDelay.Tp = Tacn.OutEngineDelayTp;


            if ((Tacn.CurrentDrivingState == Transmission.DrivingState.Brake) ||
                (Tacn.CurrentDrivingState == Transmission.DrivingState.Drive))
            {
                if ((Tacn.CurrentGearPosition == Transmission.GearPositionState.Mission1) ||
                    (Tacn.CurrentGearPosition == Transmission.GearPositionState.Mission2))
                {
                    EngineDelay.Reset(Tacn.OutRpm);
                }
            }


            EngineDelay.Calculate(AtsSimulationEnvironment.Instance.DeltaTime);


            TurbineDelay.U = Tacn.CurrentDrivingState == Transmission.DrivingState.Synching ? 0.0 : FuelControl.OutFuelCurrent;
            TurbineDelay.Tp = 1000.0f;
            TurbineDelay.Calculate(AtsSimulationEnvironment.Instance.DeltaTime);


            var actualRpm = EngineDelay.Y;


            ElectricGovernor.InActualRpm = actualRpm;
            ElectricGovernor.InTargetRpm = targetRpm;
            ElectricGovernor.Update();


            Tacn.InRpm = actualRpm;
            Tacn.Update(AtsSimulationEnvironment.Instance.DeltaTime);


            EngineNoise.DirectionMixtureRatio = (float)(fuelInjectionCurrent * 2.0);
            EngineNoise.Position = (float)Tacn.OutRpm;
            DebugForm.SetText(DebugForm.FuelInjectionCurrent, fuelInjectionCurrent.ToString("F3"));
            DebugForm.SetText(DebugForm.EngineRpm, EngineNoise.Position.ToString("0000"));


            GearNoise.DirectionMixtureRatio = 1.0f;
            GearNoise.Position = AtsSimulationEnvironment.Instance.CurrentStates.AbsoluteVelocity;
            GearNoise.Volume = AtsSimulationEnvironment.Instance.CurrentStates.MainCircuitCurrent / 100.0f;


            ExhaustNoise.DirectionMixtureRatio = (float)fuelInjectionCurrent;
            ExhaustNoise.Position = (float)Tacn.OutRpm;
            ExhaustNoise.Volume = (float)fuelInjectionCurrent;

            TurbineNoise.DirectionMixtureRatio = (float)fuelInjectionCurrent;
            TurbineNoise.Position = (float)TurbineDelay.Y;
            TurbineNoise.Volume = 1.0f;


            switch (Tacn.CurrentGearPosition)
            {
                case Transmission.GearPositionState.Mission1:
                    TransferGear1Noise.DirectionMixtureRatio = (float)fuelInjectionCurrent;
                    TransferGear1Noise.Position = (float)Tacn.OutRpm;
                    TransferGear1Noise.Volume = (float)Tacn.OutTransferGearNoiseVolume;

                    TransferGear2Noise.Volume = 0.0f;
                    break;
                case Transmission.GearPositionState.Mission2:
                    TransferGear1Noise.Volume = 0.0f;

                    TransferGear2Noise.DirectionMixtureRatio = (float)fuelInjectionCurrent;
                    TransferGear2Noise.Position = (float)Tacn.OutRpm;
                    TransferGear2Noise.Volume = (float)Tacn.OutTransferGearNoiseVolume;
                    break;
                default:
                    TransferGear1Noise.Volume = 0.0f;
                    TransferGear2Noise.Volume = 0.0f;
                    break;
            }


            EngineNoise.Update();
            GearNoise.Update();
            TransferGear1Noise.Update();
            TransferGear2Noise.Update();
            ExhaustNoise.Update();
            TurbineNoise.Update();


            AtsSimulationEnvironment.Instance.ControlHandle.TractionPosition = Tacn.OutTractionPosition;
            AtsSimulationEnvironment.Instance.ControlHandle.ReverserPosition = Tacn.OutReverserPosition;

            if (DebugForm.Visible)
            {
                DebugForm.Update();
            }
        }
    }
}
