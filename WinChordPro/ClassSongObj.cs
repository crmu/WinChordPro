using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysIO = System.IO;

namespace WinChordPro
{
    public class SongObj 
    {
        public string filePath { get; set; }
        public string fileName { get; set; }
        public string parentFolder { get; set; }
        public string key { get; set; }
        private int keyChange { get; set; }
        public string tempo { get; set; }
        public List<string> originalLines { get; set; }
        //public List<string> processedText { get; set; }
        internal List<Tuple<string, string>> processedLines { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        private static Log l = new Log();
        internal static char padChar = ' '; 

        public SongObj(){
            initialize();
        }

        private void initialize(){
            filePath = "";
            fileName = "";
            parentFolder = "";
            key = "";
            keyChange = 0;
            title = "";
            subtitle = "";
            tempo = "";
        }

        public SongObj(string fullPathToSong){
            initialize();
            filePath = fullPathToSong;
            fileName = SysIO.Path.GetFileName(filePath);
            parentFolder = SysIO.Path.GetDirectoryName(filePath);
            title = findTitle();
        }

        public int raiseKey()
        {
            if (keyChange == 5)
            {
                keyChange = -5;
            }
            else
            {
                keyChange++;
            }
            return keyChange;
        }

        public int lowerKey()
        {
            if (keyChange == -5)
            {
                keyChange = 5;
            }
            else
            {
                keyChange--;
            }
            return keyChange;
        }

        public string findTitle(){
            string tempTitle = "";
            originalLines = SysIO.File.ReadAllLines(filePath).ToList();
            foreach (string line in originalLines)
            {
                if (tempTitle == "")
                {
                    tempTitle = findValue(line, "{title");
                    if (tempTitle != "")
                        return tempTitle;
                    tempTitle = findValue(line, "{t:");
                    if (tempTitle != "")
                        return tempTitle;
                }
            }
            return SysIO.Path.GetFileNameWithoutExtension(fileName);
        }

        public void loadSong(){
            key = "";
            //keyChange = 0;
            title = "";
            subtitle = "";
            tempo = "";
            processedLines = processSong(originalLines);
            if (title == "")
                title = SysIO.Path.GetFileNameWithoutExtension(fileName);
        }

        private List<Tuple<string,string>> processSong(List<String> linesFromFile){
            Tuple<string, string> tempTuple = null;
            List<Tuple<string, string>> newLines = new List<Tuple<string, string>>();

            foreach(string line in linesFromFile) 
            {
                if (title == "")
                {
                    title = findValue(line, "{title");
                    if (title != "")
                        continue;
                    title = findValue(line, "{t:");
                    if (title != "")
                        continue;
                } 
                
                if (key == "")
                {
                    key = findValue(line, "{key");
                    if (key != "") 
                        continue;
                }

                if (subtitle == "")
                {
                    subtitle = findValue(line, "{subtitle");
                    if (subtitle != "")
                        continue;
                }
                if (tempo == "")
                {
                    tempo = findValue(line, "{tempo");
                    if (tempo != "")
                        continue;
                }
                tempTuple = null;
                tempTuple = separateChordAndLyricLines(line);
                if (tempTuple == null)
                    newLines.Add(new Tuple<string, string>("", line));
                else
                    newLines.Add(tempTuple);
            }
            return newLines;
        }

        private Tuple<string, string> separateChordAndLyricLines(string lineText)
        {
            Tuple<string, string> separatedLines = null;
            //string chordLine = "";
            StringBuilder newChordLine = new StringBuilder("");
            //string lyricLine = "";
            StringBuilder newLyricLine = new StringBuilder("");

            //string workLine = lineText.Replace("\t", "  ");
            StringBuilder newWorkLine = new StringBuilder(lineText.Replace("\t", "  "));

            //int indexOfLeftBracket = workLine.IndexOf('[');
            int indexOfLeftBracket = newWorkLine.ToString().IndexOf('[');
            if (indexOfLeftBracket < 0)
            {
                l.log("[" + fileName + "] Could not find '[' in [" + lineText + "]");
                return null;
            }

            int i = 0;
            int chordLength = 0;
            int indexOfChord = 0;
            //while(workLine.Length > 0)
            while (newWorkLine.Length > 0)
            {
                int padLength = 0;
                //indexOfLeftBracket = workLine.IndexOf('[');
                indexOfLeftBracket = newWorkLine.ToString().IndexOf('[');
                l.log("currentIndex [" + indexOfLeftBracket + "]");
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");
                if (indexOfLeftBracket < 0)
                {
                    //lyricLine += workLine;
                    newLyricLine.Append(newWorkLine);
                    break;
                }

                newLyricLine.Append(newWorkLine.ToString().Substring(0, indexOfLeftBracket));
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");


                int indexOfRighBracket = newWorkLine.ToString().IndexOf(']', indexOfLeftBracket);
                if (indexOfRighBracket < 0)
                {
                    l.log("Song[" + fileName + "] has missing closing ']' in line[" + lineText + "]");
                    newLyricLine.Append(newWorkLine);
                    l.log("Compare chord [" + newChordLine + "]");
                    l.log("To Lyricsline [" + newLyricLine + "]");

                    break;
                }

                indexOfChord = indexOfLeftBracket + 1;
                int endOfChord = indexOfRighBracket - 1;

                //In the 1st iteration chordLength should be 0 so will be no padding to lyricLine?
                padLength = indexOfLeftBracket - chordLength;
                if (padLength < 1 && i > 0)
                {
                    //lyricLine = lyricLine.PadRight(lyricLine.Length + (padLength * -1) + 1, padChar);
                    newLyricLine.Append(padChar, newLyricLine.Length + (padLength * -1) + 1);
                    padLength = 1;
                }

                //chordLine = chordLine.PadRight(chordLine.Length + padLength, padChar);
                newChordLine.Append(padChar, newChordLine.Length + padLength);
                l.log("adding #[" + padLength + "] of spaces");
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");

                chordLength = endOfChord - indexOfChord + 1;

                string chordText = newWorkLine.ToString().Substring(indexOfChord, endOfChord - indexOfChord + 1);
                l.log("chordText [" + chordText + "]");
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");
                
                //Transpose Chord found
                //keyChange = 1;
                if (keyChange > 0)
                {
                    Chord originalKey = new Chord(chordText);
                    Chord newKey = new Chord(originalKey.raiseKey(keyChange));
                    chordText = newKey.ToString();

                }
                else if (keyChange < 0)
                {
                    Chord originalKey = new Chord(chordText);
                    int keyChangeSteps = Math.Abs(keyChange);
                    Chord newKey = new Chord(originalKey.lowerKey(keyChangeSteps));
                    chordText = newKey.ToString();
                }

                //End transpose
                newChordLine.Append(chordText);
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");
                
                newWorkLine.Remove(0, indexOfRighBracket + 1);
                l.log("workLine [" + newWorkLine + "]");
                l.log("Compare chord [" + newChordLine + "]");
                l.log("To Lyricsline [" + newLyricLine + "]");
                i++;
                if (i > 20)
                    break;
            }

            separatedLines = new Tuple<string, string>(newChordLine.ToString(), newLyricLine.ToString());
            return separatedLines;
        }

        private string findValue(string lineText, string textToFind)
        {
            l.log("findValue [" + textToFind + "] within [" + lineText + "]");
            int index = lineText.IndexOf(textToFind);
            if (index < 0)
            {
                return "";
            }

            int itemEnd = lineText.IndexOf('}', index);
            int len = itemEnd - index + 1;
            string itemText = lineText.Substring(index, itemEnd - index + 1);

            int indexOfColon = itemText.IndexOf(':');
            if (indexOfColon < 0)
            {
                l.log("[" + fileName + "] Could not find ':' in[" + itemText + "]");

                return "";
            }
            return itemText.Substring(indexOfColon + 1, itemText.Length - indexOfColon - 2);
        }

    }
}
