using System;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Haptics;
using UnityEngine.Scripting;
using System.Runtime.CompilerServices;
using static UnityEngine.InputSystem.HID.HID;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.Management;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace UnityEngine.InputSystem.DualShock.Extended{


    /// <summary>
    /// PS4 output report sent as command to HID backend.
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = kSize)]
    internal unsafe struct DualShockHIDBTOutputReport : IInputDeviceCommandInfo
    {
        public static FourCC Type => new FourCC('H', 'I', 'D', 'O');
        public const int BaseCommandSize = 0;

        internal const int kSize = BaseCommandSize + 78;
        internal const int kReportId = 0x11;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags", Justification = "No better term for underlying data.")]
        [Flags]
        public enum Flags
        {
            Rumble = 0x1,
            Color = 0x2
        }

        //[FieldOffset(0)] public InputDeviceCommand baseCommand;
        [FieldOffset(0)] public byte typeData;

        [FieldOffset(BaseCommandSize + 0)] public byte reportId;
        [FieldOffset(BaseCommandSize + 1)] public byte reportId2;
        //[FieldOffset(InputDeviceCommand.BaseCommandSize + 2)] public byte unknown0;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "flags", Justification = "No better term for underlying data.")]
        [FieldOffset(BaseCommandSize + 3)] public byte flags;
        [FieldOffset(BaseCommandSize + 4)] public fixed byte unknown1[2];
        [FieldOffset(BaseCommandSize + 6)] public byte highFrequencyMotorSpeed;
        [FieldOffset(BaseCommandSize + 7)] public byte lowFrequencyMotorSpeed;
        [FieldOffset(BaseCommandSize + 8)] public byte redColor;
        [FieldOffset(BaseCommandSize + 9)] public byte greenColor;
        [FieldOffset(BaseCommandSize + 10)] public byte blueColor;
        [FieldOffset(BaseCommandSize + 11)] public fixed byte unknown2[64];

        public FourCC typeStatic => Type;

        public void SetMotorSpeeds(float lowFreq, float highFreq)
        {
            flags |= (byte)Flags.Rumble;
            lowFrequencyMotorSpeed = (byte)Mathf.Clamp(lowFreq * 255, 0, 255);
            highFrequencyMotorSpeed = (byte)Mathf.Clamp(highFreq * 255, 0, 255);
        }

        public void SetColor(Color color)
        {
            flags |= (byte)Flags.Color;
            redColor = (byte)Mathf.Clamp(color.r * 255, 0, 255);
            greenColor = (byte)Mathf.Clamp(color.g * 255, 0, 255);
            blueColor = (byte)Mathf.Clamp(color.b * 255, 0, 255);
        }

        public static DualShockHIDBTOutputReport Create()
        {
            //SafeFileHandle hidHandle = CreateFile(devicePathName, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, 3, 0, 0);
            return new DualShockHIDBTOutputReport
            {
                //baseCommand = new InputDeviceCommand(Type, kSize),
                reportId = 0x11,
                reportId2 = 0x80,
                flags = 0xff,
            };
        }
        //internal const uint GENERIC_READ = 0x80000000;
        //internal const uint GENERIC_WRITE = 0x40000000;
        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);

    }


    // We receive data as raw HID input reports. This struct
    // describes the raw binary format of such a report.
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    struct DualShock4HIDInputReport : IInputStateTypeInfo
    {
        // Because all HID input reports are tagged with the 'HID ' FourCC,
        // this is the format we need to use for this state struct.
        public FourCC format => new FourCC('H', 'I', 'D', ' ');

        // HID input reports can start with an 8-bit report ID. It depends on the device
        // whether this is present or not. On the PS4 DualShock controller, it is
        // present. We don't really need to add the field, but let's do so for the sake of
        // completeness. This can also help with debugging.
        [FieldOffset(0)] public byte reportId;

        // The InputControl annotations here probably look a little scary, but what we do
        // here is relatively straightforward. The fields we add we annotate with
        // [FieldOffset] to force them to the right location, and then we add InputControl
        // to attach controls to the fields. Each InputControl attribute can only do one of
        // two things: either it adds a new control or it modifies an existing control.
        // Given that our layout is based on Gamepad, almost all the controls here are
        // inherited from Gamepad, and we just modify settings on them.

        [InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "leftStick/x", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/left", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/right", offset = 0, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [InputControl(name = "leftStick/y", offset = 1, format = "BYTE",
            parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "leftStick/up", offset = 1, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "leftStick/down", offset = 1, format = "BYTE",
            parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(1)] public byte leftStickX;
        [FieldOffset(2)] public byte leftStickY;

        [InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
        [InputControl(name = "rightStick/x", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/left", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/right", offset = 0, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1")]
        [InputControl(name = "rightStick/y", offset = 1, format = "BYTE", parameters = "invert,normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5")]
        [InputControl(name = "rightStick/up", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0,clampMax=0.5,invert")]
        [InputControl(name = "rightStick/down", offset = 1, format = "BYTE", parameters = "normalize,normalizeMin=0,normalizeMax=1,normalizeZero=0.5,clamp,clampMin=0.5,clampMax=1,invert=false")]
        [FieldOffset(3)] public byte rightStickX;
        [FieldOffset(4)] public byte rightStickY;

        [InputControl(name = "dpad", format = "BIT", layout = "Dpad", sizeInBits = 4, defaultState = 8)]
        [InputControl(name = "dpad/up", format = "BIT", layout = "DiscreteButton", parameters = "minValue=7,maxValue=1,nullValue=8,wrapAtValue=7", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/right", format = "BIT", layout = "DiscreteButton", parameters = "minValue=1,maxValue=3", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/down", format = "BIT", layout = "DiscreteButton", parameters = "minValue=3,maxValue=5", bit = 0, sizeInBits = 4)]
        [InputControl(name = "dpad/left", format = "BIT", layout = "DiscreteButton", parameters = "minValue=5, maxValue=7", bit = 0, sizeInBits = 4)]
        [InputControl(name = "buttonWest", displayName = "Square", bit = 4)]
        [InputControl(name = "buttonSouth", displayName = "Cross", bit = 5)]
        [InputControl(name = "buttonEast", displayName = "Circle", bit = 6)]
        [InputControl(name = "buttonNorth", displayName = "Triangle", bit = 7)]
        [FieldOffset(5)] public byte buttons1;

        [InputControl(name = "leftShoulder", bit = 0)]
        [InputControl(name = "rightShoulder", bit = 1)]
        [InputControl(name = "leftTriggerButton", layout = "Button", bit = 2)]
        [InputControl(name = "rightTriggerButton", layout = "Button", bit = 3)]
        [InputControl(name = "select", displayName = "Share", bit = 4)]
        [InputControl(name = "start", displayName = "Options", bit = 5)]
        [InputControl(name = "leftStickPress", bit = 6)]
        [InputControl(name = "rightStickPress", bit = 7)]
        [FieldOffset(6)] public byte buttons2;

        [InputControl(name = "systemButton", layout = "Button", displayName = "System", bit = 0)]
        [InputControl(name = "touchpadButton", layout = "Button", displayName = "Touchpad Press", bit = 1)]
        [FieldOffset(7)] public byte buttons3;

        [InputControl(name = "leftTrigger", format = "BYTE")]
        [FieldOffset(8)] public byte leftTrigger;

        [InputControl(name = "rightTrigger", format = "BYTE")]
        [FieldOffset(9)] public byte rightTrigger;

        [InputControl(name = "gyroX", layout = "Axis", format = "SHRT")]
        [FieldOffset(13)] public short gyroX;
        [InputControl(name = "gyroY", layout = "Axis", format = "SHRT")]
        [FieldOffset(15)] public short gyroY;
        [InputControl(name = "gyroZ", layout = "Axis", format = "SHRT")]
        [FieldOffset(17)] public short gyroZ;

        [InputControl(name = "batteryCharge", layout = "Integer", format = "BIT", sizeInBits = 4)]
        [InputControl(name = "batteryCharging", layout = "Integer", format = "BIT", bit = 4, sizeInBits = 1)]
        [FieldOffset(30)] public byte batteryLevel;

        [InputControl(name = "touchOffset", layout = "Integer", format = "BYTE")]
        [FieldOffset(33)] public byte touchOffset;

        [InputControl(name = "touch1", layout = "Integer", format = "INT")]
        [FieldOffset(35)] public int touch1;
        [InputControl(name = "touch2", layout = "Integer", format = "INT")]
        [FieldOffset(39)] public int touch2;

    }
#if UNITY_EDITOR
    [InitializeOnLoad] // Make sure static constructor is called during startup.
#endif
    public class DualShock4GamepadInitializer
    {
        static DualShock4GamepadInitializer()
        {
            //InputSystem.RegisterLayout<DualShockGamepad>(null, null);
            // Alternatively, you can also match by PID and VID, which is generally
            // more reliable for HIDs.
            //InputSystem.RemoveLayout("DualShock4GamepadHID");
            
            /*InputSystem.onDeviceChange += InputSystem_onDeviceChange;
            InputSystem.RegisterLayoutBuilder(
               () =>
               {
                   return InputControlLayout.FromType(null,typeof(DualShock4GamepadHIDExtended));
                   var builder = new InputControlLayout.Builder()
                       .WithType<DualShock4GamepadHIDExtended>();
                       
                   return builder.Build();
               }, "DualShock4GamepadHID");*/
            InputSystem.RegisterLayout<DualShock4GamepadHIDExtended>(
                name: "DualShock4GamepadHID",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithCapability("vendorId", 0x54C) // Sony Entertainment.
                    .WithCapability("productId", 0x9CC)); // Wireless controller.
            InputSystem.RegisterLayout<DualShock4GamepadHIDExtended>(
                name: "DualShock4GamepadHID",
                matches: new InputDeviceMatcher()
                    .WithInterface("HID")
                    .WithManufacturer("Sony.+Entertainment")
                    .WithProduct("Wireless Controller"));
        }


		private static void InputSystem_onDeviceChange(InputDevice arg1, InputDeviceChange arg2)
		{
            switch(arg2){
                case InputDeviceChange.Added:
                    Debug.Log(arg1.description.ToJson());
                    break;
			}
        }

		// In the Player, to trigger the calling of the static constructor,
		// create an empty method annotated with RuntimeInitializeOnLoadMethod.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init() {
        }
    }

	public class BatteryControl : InputControl<byte>
	{
		public override unsafe byte ReadUnprocessedValueFromState(void* statePtr)
		{
            return (byte)m_StateBlock.ReadInt(statePtr);
        }
	}

    [InputControlLayout(stateType = typeof(DualShock4HIDInputReport), hideInUI = true)]
    [Preserve]
    public class DualShock4GamepadHIDExtended : DualShock4GamepadHID
    {
		public IntegerControl batteryCharge { get; private set; }
        public IntegerControl batteryCharging { get; private set; }

        IntegerControl touch1, touch2;

        protected override void FinishSetup()
        {
            //Debug.Log(layout);
            bool _isBluetooth = description.version!="256";// HIDDD.inputReportSize > 64;
            if (_isBluetooth)
            {
                HIDDeviceDescriptor HIDDD = HIDDeviceDescriptor.FromJson(description.capabilities);
                var prop = typeof(InputControl).GetField("m_StateBlock", BindingFlags.NonPublic | BindingFlags.Instance);
                InputStateBlock isb = stateBlock;
                isb.sizeInBits += 16;
                prop.SetValue(this, isb);
                foreach (InputControl item in this.allControls)
                {
                    isb = item.stateBlock;
                    isb.byteOffset += 2;
                    prop.SetValue(item, isb);
                }
            }
            batteryCharge = GetChildControl<IntegerControl>("batteryCharge");
            batteryCharging = GetChildControl<IntegerControl>("batteryCharging");
            touch1 = GetChildControl<IntegerControl>("touch1");
            touch2 = GetChildControl<IntegerControl>("touch2");
            base.FinishSetup();
            //Debug.Log(device.description.);
        }
        public void SetBTMode(){
            throw new Exception("Unity not support send data for not USB HID");
            DualShockHIDBTOutputReport command = DualShockHIDBTOutputReport.Create();
            command.SetColor(Color.yellow);
            base.ExecuteCommand<DualShockHIDBTOutputReport>(ref command);
        }

        public bool GetTouch(byte id,out byte touchID,out Vector2Int pos){
            switch(id)
            {
                case 0: {
                        int i = touch1.ReadValue();
                        touchID = (byte)(i & 127);
                        pos = new Vector2Int((i>>8)&4095,(i>>20)&4095);
                        return ((i >> 7) & 1) != 1;
                    }
                case 1:
                    {
                        int i = touch2.ReadValue();
                        touchID = (byte)(i & 127);
                        pos = new Vector2Int((i >> 8) & 4095, (i >> 20) & 4095);
                        return ((i >> 7) & 1) != 1;
                    }
                default:
                    touchID = 255;
                    pos = Vector2Int.zero;
                    return false;
			}
		}
	}
}