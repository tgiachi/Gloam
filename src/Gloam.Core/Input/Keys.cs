namespace Gloam.Core.Input;

/// <summary>
///     Complete set of input keys that work across different input systems
///     Key codes based on Virtual-Key codes (Windows) for compatibility
/// </summary>
public static class Keys
{
    #region Control Keys

    public static readonly InputKeyData None = new(0, "None");
    public static readonly InputKeyData Cancel = new(3, "Cancel");
    public static readonly InputKeyData Backspace = new(8, "Backspace");
    public static readonly InputKeyData Tab = new(9, "Tab");
    public static readonly InputKeyData LineFeed = new(10, "LineFeed");
    public static readonly InputKeyData Clear = new(12, "Clear");
    public static readonly InputKeyData Enter = new(13, "Enter");
    public static readonly InputKeyData Return = new(13, "Return");
    public static readonly InputKeyData Pause = new(19, "Pause");
    public static readonly InputKeyData CapsLock = new(20, "CapsLock");
    public static readonly InputKeyData Capital = new(20, "Capital");
    public static readonly InputKeyData Escape = new(27, "Escape");
    public static readonly InputKeyData Space = new(32, "Space");

    #endregion

    #region Navigation Keys

    public static readonly InputKeyData PageUp = new(33, "PageUp");
    public static readonly InputKeyData PageDown = new(34, "PageDown");
    public static readonly InputKeyData End = new(35, "End");
    public static readonly InputKeyData Home = new(36, "Home");
    public static readonly InputKeyData Left = new(37, "Left");
    public static readonly InputKeyData Up = new(38, "Up");
    public static readonly InputKeyData Right = new(39, "Right");
    public static readonly InputKeyData Down = new(40, "Down");
    public static readonly InputKeyData Select = new(41, "Select");
    public static readonly InputKeyData Print = new(42, "Print");
    public static readonly InputKeyData Execute = new(43, "Execute");
    public static readonly InputKeyData PrintScreen = new(44, "PrintScreen");
    public static readonly InputKeyData Insert = new(45, "Insert");
    public static readonly InputKeyData Delete = new(46, "Delete");
    public static readonly InputKeyData Help = new(47, "Help");

    #endregion

    #region Number Keys (Main Row)

    public static readonly InputKeyData D0 = new(48, "0");
    public static readonly InputKeyData D1 = new(49, "1");
    public static readonly InputKeyData D2 = new(50, "2");
    public static readonly InputKeyData D3 = new(51, "3");
    public static readonly InputKeyData D4 = new(52, "4");
    public static readonly InputKeyData D5 = new(53, "5");
    public static readonly InputKeyData D6 = new(54, "6");
    public static readonly InputKeyData D7 = new(55, "7");
    public static readonly InputKeyData D8 = new(56, "8");
    public static readonly InputKeyData D9 = new(57, "9");

    #endregion

    #region Letter Keys

    public static readonly InputKeyData A = new(65, "A");
    public static readonly InputKeyData B = new(66, "B");
    public static readonly InputKeyData C = new(67, "C");
    public static readonly InputKeyData D = new(68, "D");
    public static readonly InputKeyData E = new(69, "E");
    public static readonly InputKeyData F = new(70, "F");
    public static readonly InputKeyData G = new(71, "G");
    public static readonly InputKeyData H = new(72, "H");
    public static readonly InputKeyData I = new(73, "I");
    public static readonly InputKeyData J = new(74, "J");
    public static readonly InputKeyData K = new(75, "K");
    public static readonly InputKeyData L = new(76, "L");
    public static readonly InputKeyData M = new(77, "M");
    public static readonly InputKeyData N = new(78, "N");
    public static readonly InputKeyData O = new(79, "O");
    public static readonly InputKeyData P = new(80, "P");
    public static readonly InputKeyData Q = new(81, "Q");
    public static readonly InputKeyData R = new(82, "R");
    public static readonly InputKeyData S = new(83, "S");
    public static readonly InputKeyData T = new(84, "T");
    public static readonly InputKeyData U = new(85, "U");
    public static readonly InputKeyData V = new(86, "V");
    public static readonly InputKeyData W = new(87, "W");
    public static readonly InputKeyData X = new(88, "X");
    public static readonly InputKeyData Y = new(89, "Y");
    public static readonly InputKeyData Z = new(90, "Z");

    #endregion

    #region Windows Keys

    public static readonly InputKeyData LeftWindows = new(91, "LeftWindows");
    public static readonly InputKeyData RightWindows = new(92, "RightWindows");
    public static readonly InputKeyData Apps = new(93, "Apps");
    public static readonly InputKeyData Sleep = new(95, "Sleep");

    #endregion

    #region Numeric Keypad

    public static readonly InputKeyData NumPad0 = new(96, "NumPad0");
    public static readonly InputKeyData NumPad1 = new(97, "NumPad1");
    public static readonly InputKeyData NumPad2 = new(98, "NumPad2");
    public static readonly InputKeyData NumPad3 = new(99, "NumPad3");
    public static readonly InputKeyData NumPad4 = new(100, "NumPad4");
    public static readonly InputKeyData NumPad5 = new(101, "NumPad5");
    public static readonly InputKeyData NumPad6 = new(102, "NumPad6");
    public static readonly InputKeyData NumPad7 = new(103, "NumPad7");
    public static readonly InputKeyData NumPad8 = new(104, "NumPad8");
    public static readonly InputKeyData NumPad9 = new(105, "NumPad9");
    public static readonly InputKeyData Multiply = new(106, "Multiply");
    public static readonly InputKeyData Add = new(107, "Add");
    public static readonly InputKeyData Separator = new(108, "Separator");
    public static readonly InputKeyData Subtract = new(109, "Subtract");
    public static readonly InputKeyData DecimalPoint = new(110, "DecimalPoint");
    public static readonly InputKeyData Divide = new(111, "Divide");

    #endregion

    #region Function Keys

    public static readonly InputKeyData F1 = new(112, "F1");
    public static readonly InputKeyData F2 = new(113, "F2");
    public static readonly InputKeyData F3 = new(114, "F3");
    public static readonly InputKeyData F4 = new(115, "F4");
    public static readonly InputKeyData F5 = new(116, "F5");
    public static readonly InputKeyData F6 = new(117, "F6");
    public static readonly InputKeyData F7 = new(118, "F7");
    public static readonly InputKeyData F8 = new(119, "F8");
    public static readonly InputKeyData F9 = new(120, "F9");
    public static readonly InputKeyData F10 = new(121, "F10");
    public static readonly InputKeyData F11 = new(122, "F11");
    public static readonly InputKeyData F12 = new(123, "F12");
    public static readonly InputKeyData F13 = new(124, "F13");
    public static readonly InputKeyData F14 = new(125, "F14");
    public static readonly InputKeyData F15 = new(126, "F15");
    public static readonly InputKeyData F16 = new(127, "F16");
    public static readonly InputKeyData F17 = new(128, "F17");
    public static readonly InputKeyData F18 = new(129, "F18");
    public static readonly InputKeyData F19 = new(130, "F19");
    public static readonly InputKeyData F20 = new(131, "F20");
    public static readonly InputKeyData F21 = new(132, "F21");
    public static readonly InputKeyData F22 = new(133, "F22");
    public static readonly InputKeyData F23 = new(134, "F23");
    public static readonly InputKeyData F24 = new(135, "F24");

    #endregion

    #region Lock Keys

    public static readonly InputKeyData NumLock = new(144, "NumLock");
    public static readonly InputKeyData ScrollLock = new(145, "ScrollLock");

    #endregion

    #region Modifier Keys

    public static readonly InputKeyData LeftShift = new(160, "LeftShift");
    public static readonly InputKeyData RightShift = new(161, "RightShift");
    public static readonly InputKeyData LeftCtrl = new(162, "LeftCtrl");
    public static readonly InputKeyData RightCtrl = new(163, "RightCtrl");
    public static readonly InputKeyData LeftAlt = new(164, "LeftAlt");
    public static readonly InputKeyData RightAlt = new(165, "RightAlt");

    #endregion

    #region Browser Keys

    public static readonly InputKeyData BrowserBack = new(166, "BrowserBack");
    public static readonly InputKeyData BrowserForward = new(167, "BrowserForward");
    public static readonly InputKeyData BrowserRefresh = new(168, "BrowserRefresh");
    public static readonly InputKeyData BrowserStop = new(169, "BrowserStop");
    public static readonly InputKeyData BrowserSearch = new(170, "BrowserSearch");
    public static readonly InputKeyData BrowserFavorites = new(171, "BrowserFavorites");
    public static readonly InputKeyData BrowserHome = new(172, "BrowserHome");

    #endregion

    #region Volume Keys

    public static readonly InputKeyData VolumeMute = new(173, "VolumeMute");
    public static readonly InputKeyData VolumeDown = new(174, "VolumeDown");
    public static readonly InputKeyData VolumeUp = new(175, "VolumeUp");

    #endregion

    #region Media Keys

    public static readonly InputKeyData MediaNext = new(176, "MediaNext");
    public static readonly InputKeyData MediaPrevious = new(177, "MediaPrevious");
    public static readonly InputKeyData MediaStop = new(178, "MediaStop");
    public static readonly InputKeyData MediaPlay = new(179, "MediaPlay");

    #endregion

    #region Launch Keys

    public static readonly InputKeyData LaunchMail = new(180, "LaunchMail");
    public static readonly InputKeyData LaunchMediaSelect = new(181, "LaunchMediaSelect");
    public static readonly InputKeyData LaunchApp1 = new(182, "LaunchApp1");
    public static readonly InputKeyData LaunchApp2 = new(183, "LaunchApp2");

    #endregion

    #region Punctuation and Symbols

    public static readonly InputKeyData Semicolon = new(186, "Semicolon");       // ;:
    public static readonly InputKeyData Plus = new(187, "Plus");                 // =+
    public static readonly InputKeyData Comma = new(188, "Comma");               // ,<
    public static readonly InputKeyData Minus = new(189, "Minus");               // -_
    public static readonly InputKeyData Period = new(190, "Period");             // .>
    public static readonly InputKeyData Slash = new(191, "Slash");               // /?
    public static readonly InputKeyData Tilde = new(192, "Tilde");               // `~
    public static readonly InputKeyData OpenBracket = new(219, "OpenBracket");   // [{
    public static readonly InputKeyData Backslash = new(220, "Backslash");       // \|
    public static readonly InputKeyData CloseBracket = new(221, "CloseBracket"); // ]}
    public static readonly InputKeyData Quote = new(222, "Quote");               // '"

    #endregion

    #region Extended Keys

    public static readonly InputKeyData Oem102 = new(226, "Oem102"); // Various extended keys
    public static readonly InputKeyData Process = new(229, "Process");
    public static readonly InputKeyData Packet = new(231, "Packet");
    public static readonly InputKeyData Attention = new(246, "Attention");
    public static readonly InputKeyData CrSel = new(247, "CrSel");
    public static readonly InputKeyData ExSel = new(248, "ExSel");
    public static readonly InputKeyData EraseEndOfFile = new(249, "EraseEndOfFile");
    public static readonly InputKeyData Play = new(250, "Play");
    public static readonly InputKeyData Zoom = new(251, "Zoom");
    public static readonly InputKeyData NoName = new(252, "NoName");
    public static readonly InputKeyData Pa1 = new(253, "Pa1");
    public static readonly InputKeyData OemClear = new(254, "OemClear");

    #endregion

    #region Alias for Common Keys (Convenience)

    // Movement aliases
    public static readonly InputKeyData UpArrow = Up;
    public static readonly InputKeyData DownArrow = Down;
    public static readonly InputKeyData LeftArrow = Left;
    public static readonly InputKeyData RightArrow = Right;

    // Common aliases
    public static readonly InputKeyData Esc = Escape;
    public static readonly InputKeyData Ctrl = LeftCtrl;
    public static readonly InputKeyData Shift = LeftShift;
    public static readonly InputKeyData Alt = LeftAlt;
    public static readonly InputKeyData Win = LeftWindows;
    public static readonly InputKeyData Menu = Apps;

    // Symbol aliases (for readability in roguelike context)
    public static readonly InputKeyData Question = Slash;     // ? for help
    public static readonly InputKeyData LessThan = Comma;     // < for stairs up
    public static readonly InputKeyData GreaterThan = Period; // > for stairs down
    public static readonly InputKeyData Colon = Semicolon;    // : for examination

    #endregion
}
