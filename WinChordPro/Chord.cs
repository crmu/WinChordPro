using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinChordPro
{
    public enum Key
    {
        None = -1,
        A = 0,
        Bb = 1,
        B = 2,
        C = 3,
        Db = 4,
        D = 5 ,
        Eb = 6,
        E = 7,
        F = 8,
        Gb = 9,
        G = 10,
        Ab = 11
    }

    internal class Chord
    {
        private static Log l = new Log();
        public Key majorKey { get; set; }
        public string entireChord { get; set; }
        public List<string> chordParts { get; set; }

        public Chord()
        {
            majorKey = Key.None;
        }
        
        public Chord(Key aKey)
        {
            majorKey = aKey;
        }

        public Chord(string entireChord)
        {
            chordParts = parseChord(entireChord);
            majorKey = getChordRoot(entireChord);

        }

        public List<string> parseChord(string chordString)
        {
            List<string> splitChord = new List<string>();

            string workString = "";
            foreach (char aChar in chordString)
            {
                if (aChar == '/')
                {
                    splitChord.Add(getChordRoot(workString).ToString());
                    splitChord.Add(getRestOfChord(workString).ToString());
                    splitChord.Add("/");
                    workString = "";
                    continue;
                }
                else if (aChar == '-')
                {
                    splitChord.Add(getChordRoot(workString).ToString());
                    splitChord.Add(getRestOfChord(workString).ToString());
                    splitChord.Add("-");
                    workString = "";
                    continue;
                }
                else
                {
                    workString += aChar;
                }
            }
            splitChord.Add(workString);

            return splitChord;
        }

        public Key findKey(string inputKey)
        {
            Key tempKey = Key.None;
            string keyFound = inputKey.Trim();

            if (keyFound.Length == 0)
            {
                return Key.None;
            }
            if (keyFound.Length > 1)
            {
                switch (keyFound.Substring(0, 2))
                {
                    case "A#":
                    case "a#":
                    case "Bb":
                        tempKey = Key.Bb;
                        break;
                    case "C#":
                    case "c#":
                    case "Db":
                    case "db":
                        tempKey = Key.Db;
                        break;
                    case "D#":
                    case "d#":
                    case "Eb":
                    case "eb":
                        tempKey = Key.Eb;
                        break;
                    case "F#":
                    case "f#":
                    case "Gb":
                    case "gb":
                        tempKey = Key.Gb;
                        break;
                    case "G#":
                    case "g#":
                    case "Ab":
                    case "ab":
                        tempKey = Key.Ab;
                        break;
                    default:
                        tempKey = Key.None;
                        break;
                }
            }
            if (tempKey == Key.None)
            {
                switch (keyFound[0])
                {
                    case 'C':
                    case 'c':
                        tempKey = Key.C;
                        break;
                    case 'D':
                    case 'd':
                        tempKey = Key.D;
                        break;
                    case 'E':
                    case 'e':
                        tempKey = Key.E;
                        break;
                    case 'F':
                    case 'f':
                        tempKey = Key.F;
                        break;
                    case 'G':
                    case 'g':
                        tempKey = Key.G;
                        break;
                    case 'A':
                    case 'a':
                        tempKey = Key.A;
                        break;
                    case 'B':
                    case 'b':
                        tempKey = Key.B;
                        break;
                    default:
                        tempKey = Key.None;
                        break;
                }
            }
            return tempKey;
        }

        public string raiseKey()
        {
            l.log("Raising Key");
            string returnString = "";
            foreach (string section in chordParts)
            {
                Key thisKey = findKey(section);
                if (thisKey == Key.None)
                    returnString += section;
                if (thisKey == Key.Ab)
                {
                    returnString += Key.A.ToString();
                }
                else
                {
                    returnString += (thisKey + 1).ToString();
                }
            }
            return returnString;
        }

        public Key raiseKey(int steps)
        {
            l.log("Raising key [" + steps + "] steps");

            if (majorKey == Key.None)
                return Key.None;

            if (steps > (int)Key.Ab)
            {
                l.log("Too many steps to raise [" + steps + "]");
                return Key.None;
            }
            Chord newKey = new Chord(majorKey);
            for (int i = 0; i < steps; i++)
            {
                newKey.raiseKey();
                l.log("Raised key to [" + newKey.ToString() + "]");
            }
            return newKey.majorKey;
        }

        public Key lowerKey()
        {
            l.log("Lowering Key");
            if (majorKey == Key.None)
                return Key.None;

            if (majorKey == Key.A)
            {
                majorKey = Key.Ab;
                return majorKey;
            }
            majorKey = majorKey - 1;
            return majorKey;
        }

        public Key lowerKey(int steps)
        {
            l.log("Lowering key [" + steps + "] steps");

            if (majorKey == Key.None)
                return Key.None;

            if (steps > (int)Key.Ab)
            {
                l.log("Too many steps to lower [" + steps + "]");
                return Key.None;
            }

            Chord newKey = new Chord(majorKey);
            for (int i = 0; i < steps; i++)
            {
                newKey.lowerKey();
                l.log("Lowered key to [" + majorKey + "]");
            }
            return newKey.majorKey;
        }


        override public string ToString()
        {
            return Enum.GetName(typeof(Key), majorKey);
        }

        public int getKeyLength(Chord myKey)
        {
            switch (myKey.ToString())
            {
                    case "A#":
                    case "Bb":
                    case "C#":
                    case "Db":
                    case "D#":
                    case "Eb":
                    case "F#":
                    case "Gb":
                    case "G#":
                    case "Ab":
                        return 2;
                    case "C":
                    case "D":
                    case "E":
                    case "F":
                    case "G":
                    case "A":
                    case "B":
                        return 1;
                    default:
                        l.log("Invalid key[" + myKey.majorKey + "], return length of 0");
                        break;
            }
            return 0;
        }

        public Key getChordRoot(string inputChord)
        {
            return findKey(inputChord);
        }

        public string getRestOfChord(string inputChord)
        {
            int indexOfRemainder = -1;
            Key mainKey = getChordRoot(inputChord);
            indexOfRemainder = getKeyLength(new Chord(mainKey));
            if (indexOfRemainder < 0)
                return null;

            if (inputChord.IndexOf('/') >= 0)
                l.log("Unexpected / in rest of chord");
            if (inputChord.IndexOf('-') >= 0)
                l.log("Unexpected - in rest of chord");

            if (mainKey == Key.None)
            {
                l.log("Could not separate chord root from remainder in [" + inputChord + "]");
                return inputChord;
            }
            return inputChord.Substring(indexOfRemainder);
        }

     }
}
