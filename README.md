# Unity3D_DualShock4_Support
Support DualShock4 when native new InputSystem Unity3D

Attention, because unity3d have bugs with send data for not USB HID, is not fully support bluetooth mode and send data, if future Unity Team fix this bug, this code been work i hope.
And some else if connect DS4 in wiriless and run DS4Windows or other programm where can connect to DS4 and send first BT data, after this we can read data in Unity3D, but cant send =D

i dont know why unity not use "HidD_SetOutputReport"(in windows, how did in linux idk) for send data not for USB devices and i hope they fix that



my test sample

    DualShock4GamepadHIDExtended ds4;
    // Start is called before the first frame update
    void Start()
    {
        foreach(Gamepad item in Gamepad.all){
            if (item is DualShock4GamepadHIDExtended)
            {
                ds4 = item as DualShock4GamepadHIDExtended;
            }
		    }
        ds4.SetBTMode();
    }
    // Update is called once per frame
    void Update()
    {
        byte id;
        Vector2Int pos;
        Debug.Log(ds4.GetTouch(0,out id,out pos)+":"+id+":"+pos);
    }
