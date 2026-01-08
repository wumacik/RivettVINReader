using System.Collections.Generic;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace RivettVINReader
{
    [BepInPlugin("wumat.rivettvinreader", "RivettVINReader", "1.0.2")]
    public class Plugin : BaseUnityPlugin
    {
        // vibe coded ahh mod :sob:

        internal static new ManualLogSource Logger;
        private ConfigEntry<KeyboardShortcut> toggleKey;
        private bool showUI;
        private Rect windowRect;
        private string vinRaw = "UNKNOWN";
        private Dictionary<string, string> decoded = new Dictionary<string, string>();
        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"RivettVINReader is loaded | made with <3 by wumat");

            toggleKey = Config.Bind(
                "Controls",
                "ToggleVINReader",
                new KeyboardShortcut(KeyCode.V),
                "Key to toggle the RivettVINReader UI"
            );

            windowRect = new Rect(
                Screen.width / 2 - 400,
                Screen.height / 2 - 350,
                800,
                700
            );
        }
        void Update()
        {
            if (toggleKey.Value.IsDown())
            {
                showUI = !showUI;

                if (showUI)
                    ReadVINOnce();
            }
        }
        GUIStyle Bold()
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
        }
        GUIStyle Small()
        {
            return new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };
        }
        private void DrawVINWindow(int id)
        {
            GUILayout.Space(10);

            GUILayout.Label($"VIN: {vinRaw}", Bold());

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");

            foreach (var kv in decoded)
            {
                GUILayout.Label($"{kv.Key}: {kv.Value}");
            }

            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.Label($"Press {toggleKey.Value} to close", Small());

            GUI.DragWindow();
        }
        private void OnGUI()
        {
            if (!showUI || string.IsNullOrEmpty(vinRaw) || vinRaw == "UNKNOWN")
                return;

            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.85f);
            windowRect = GUI.Window(1337, windowRect, DrawVINWindow, "RivettVINReader by wumat");
        }

        void ReadVINOnce()
        {
            decoded.Clear();
            vinRaw = "UNKNOWN";

            GameObject vinPlate = GameObject.Find("VINPlate");
            if (vinPlate == null)
            {
                Logger.LogWarning("VIN UI: VINPlate not found");
                return;
            }

            BuildVINFromPlate(vinPlate);

            Logger.LogInfo($"VIN UI: VIN loaded -> {vinRaw}");
        }


        private void BuildVINFromPlate(GameObject vinPlate)
        {
            decoded.Clear();

            string Get(string name)
            {
                var t = vinPlate.transform.Find($"Render/VIN/{name}");
                if (t == null) return "";
                var tm = t.GetComponent<TextMesh>();
                return tm ? tm.text.Trim() : "";
            }

            string country = Get("Country");
            string plant = Get("AssemblyPlant");
            string model = Get("Model");
            string body = Get("Body");
            string version = Get("Version");
            string year = Get("Year");
            string month = Get("Month");
            string serial = Get("Serial");
            string drive = Get("Drive");
            string engine = Get("Engine");
            string gearbox = Get("Gearbox");
            string axleRatio = Get("AxleRatio");
            string axleLock = Get("AxleLock");
            string bodyColor = Get("ColorsBody");
            string vinyl = Get("VinylRoof");
            string interior = Get("InteriorTrim");
            string radio = Get("Radio");
            string cluster = Get("InstrumentPanel");
            string windshield = Get("Windshield");
            string seats = Get("Seats");
            string suspension = Get("Suspension");
            string brakes = Get("PowerBrakes");
            string wheels = Get("Wheels");
            string rearWindow = Get("WindowHeater");

            vinRaw =
                country + plant + model + body + version +
                year + month + serial +
                drive + engine + gearbox +
                axleRatio + axleLock +
                bodyColor + vinyl + interior +
                radio + cluster + windshield +
                seats + suspension + brakes +
                wheels + rearWindow;

            DecodeVIN(
                country, plant, model, body, version, year, month,
                serial, drive, engine, gearbox, axleRatio, axleLock,
                bodyColor, vinyl, interior, radio, cluster, windshield,
                seats, suspension, brakes, wheels, rearWindow
            );

            Logger.LogInfo($"VIN BUILT: {vinRaw}");
        }
        private void DecodeVIN(
            string country, string plant, string model, string body, string version,
            string year, string month, string serial, string drive, string engine,
            string gearbox, string axleRatio, string axleLock, string bodyColor,
            string vinyl, string interior, string radio, string cluster,
            string windshield, string seats, string suspension, string brakes,
            string wheels, string rearWindow)
        {
            decoded["Country"] = Map(country, this.country);
            decoded["Assembly Plant"] = Map(plant, this.plant);
            decoded["Model"] = "Rivett";
            decoded["Body Type"] = "2D Pillared Sedan";
            decoded["Version"] = Map(version, this.version);
            decoded["Year"] = Map(year, this.year);
            decoded["Month"] = Map(month, this.month);
            decoded["Serial Number"] = serial;
            decoded["Drive"] = Map(drive, this.drive);

            decoded["Engine"] = engine switch
            {
                "NA" => "2.0 Standard",
                "NE" => "2.0 High Performance",
                _ => engine
            };

            decoded["Gearbox"] = Map(gearbox, this.gearbox);
            decoded["Axle Ratio"] = Map(axleRatio, this.axleRatio);
            decoded["Axle Lock"] = Map(axleLock, this.axleLock);
            decoded["Body Color"] = Map(bodyColor, this.bodyColor);
            decoded["Vinyl Roof"] = Map(vinyl, vinylRoof);
            decoded["Interior Trim"] = Map(interior, this.interior);
            decoded["Radio"] = Map(radio, this.radio);
            decoded["Instrument Panel"] = Map(cluster, panel);
            decoded["Windshield"] = Map(windshield, this.windshield);
            decoded["Seats"] = Map(seats, this.seats);
            decoded["Suspension"] = Map(suspension, this.suspension);
            decoded["Brakes"] = Map(brakes, this.brakes);
            decoded["Wheels"] = Map(wheels, this.wheels);
            decoded["Rear Window"] = Map(rearWindow, this.rearWindow);
        }
        string Map(string value, Dictionary<char, string> map)
        {
            if (string.IsNullOrEmpty(value))
                return "Unknown";

            char c = value[0];
            return map.TryGetValue(c, out var result) ? result : value;
        }

        // ===================== mapping tables =====================

        Dictionary<char, string> country = new() { { 'U', "Corris Britain" } };
        Dictionary<char, string> plant = new() { { 'A', "Dagenham" }, { 'B', "Manchester" }, { 'C', "Saarlouis" }, { 'K', "Rheine" } };
        Dictionary<char, string> model = new() { { 'B', "Rivett" } };
        Dictionary<char, string> body = new() { { 'B', "2D Pillared Sedan" } };
        Dictionary<char, string> version = new() { { 'D', "L" }, { 'E', "LX" }, { 'G', "SLX" }, { 'P', "GT" } };
        Dictionary<char, string> year = new() { { 'L', "1971" }, { 'M', "1972" }, { 'N', "1973" }, { 'P', "1974 (Facelift)" }, { 'R', "1975" }, { 'S', "1976" } };
        Dictionary<char, string> month = new() { { 'C', "Jan" }, { 'K', "Feb" }, { 'D', "Mar" }, { 'E', "Apr" }, { 'L', "May" }, { 'Y', "Jun" }, { 'S', "Jul" }, { 'T', "Aug" }, { 'J', "Sep" }, { 'U', "Oct" }, { 'M', "Nov" }, { 'P', "Dec" } };
        Dictionary<char, string> drive = new() { { '1', "RWD" } };
        Dictionary<char, string> gearbox = new() { { '7', "3-spd Auto" }, { 'B', "4-spd Manual" } };
        Dictionary<char, string> axleRatio = new() { { 'S', "3.44" }, { 'B', "3.75" }, { 'C', "3.89" }, { 'N', "4.11" }, { 'E', "4.44" } };
        Dictionary<char, string> axleLock = new() { { 'A', "Open" }, { 'B', "LSD" } };
        Dictionary<char, string> bodyColor = new() {
        {'A',"Dark Grey"},{'B',"Nature White"},{'C',"Sand"},{'D',"Asphalt Grey"},
        {'E',"Blue"},{'F',"Sun Yellow"},{'G',"Dark Navy"},{'H',"Royal Red"},
        {'I',"Brown"},{'J',"Red"},{'K',"Electric Green"},{'L',"White Pearl"},
        {'M',"Spring Green"},{'R',"Purple"},{'T',"Yellow"},{'U',"Sky Blue"},
        {'V',"Orange"},{'X',"Navy Blue"},{'Y',"Special"}
    };
        Dictionary<char, string> vinylRoof = new() { { '-', "No" }, { 'A', "Black" }, { 'B', "White" }, { 'C', "Tan" }, { 'K', "Light Brown" }, { 'M', "Dark Brown" } };
        Dictionary<char, string> interior = new() { { 'N', "Red" }, { 'A', "Black" }, { 'K', "Tan" }, { 'F', "Blue" }, { 'Y', "Special" } };
        Dictionary<char, string> radio = new() { { '-', "No" }, { 'J', "Yes" } };
        Dictionary<char, string> panel = new() { { '-', "Standard" }, { 'G', "Clock" }, { 'M', "Tachometer" } };
        Dictionary<char, string> windshield = new() { { '1', "Clear" }, { '2', "Tinted" }, { 'F', "Sunstrip" } };
        Dictionary<char, string> seats = new() { { '8', "Standard" }, { 'B', "Bucket" } };
        Dictionary<char, string> suspension = new() { { 'A', "Standard" }, { 'B', "Standard + Stiffened" }, { '4', "Lowered" }, { 'M', "Lowered + Stiffened" } };
        Dictionary<char, string> brakes = new() { { '-', "Standard" }, { 'B', "Power Brakes" } };
        Dictionary<char, string> wheels = new() { { 'A', "13\" Steel" }, { 'B', "13\" + Caps" }, { '4', "14\" Sport" }, { 'M', "14\" Steel/Octo" } };
        Dictionary<char, string> rearWindow = new() { { '-', "Standard" }, { 'B', "Heated" }, { 'M', "Grille" } };
    }
}
