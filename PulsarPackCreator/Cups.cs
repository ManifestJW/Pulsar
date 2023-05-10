﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pulsar_Pack_Creator;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Diagnostics.Eventing.Reader;

namespace PulsarPackCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class Cup
        {
            public Cup(UInt32 idx)
            {
                this.idx = idx;
                slots = new byte[4] { 0x8, 0x8, 0x8, 0x8 };
                string defaultFile = "File";
                string defaultTrack = "Name";
                string defaultAuthor = "Author";
                string defaultGhost = "";
                musicSlots = new byte[4] { 0x8, 0x8, 0x8, 0x8 };
                fileNames = new string[4] { defaultFile, defaultFile, defaultFile, defaultFile };
                trackNames = new string[4] { defaultTrack, defaultTrack, defaultTrack, defaultTrack };
                authorNames = new string[4] { defaultAuthor, defaultAuthor, defaultAuthor, defaultAuthor };
                expertFileNames = new string[4, 4] {{defaultGhost, defaultGhost, defaultGhost, defaultGhost},
                                                    {defaultGhost, defaultGhost, defaultGhost, defaultGhost},
                                                    {defaultGhost, defaultGhost, defaultGhost, defaultGhost},
                                                    {defaultGhost, defaultGhost, defaultGhost,defaultGhost} };
            }
            public Cup(BigEndianReader bin) : this(0)
            {
                idx = bin.ReadUInt32();
                for (int i = 0; i < 4; i++)
                {
                    slots[i] = bin.ReadByte();
                    musicSlots[i] = bin.ReadByte();
                    bin.BaseStream.Position += 4;
                }
            }

            public UInt32 idx;
            //Data
            public byte[] slots;
            public byte[] musicSlots;
            public string[] fileNames;
            public string[] trackNames;
            public string[] authorNames;
            public string[,] expertFileNames;
        }

        private void OnCupCountChange(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Text == "" || box.Text == "0")
            {
                box.Text = "1";
                return;
            }
            UInt16 newCount = UInt16.Parse(box.Text);
            if (newCount > 1000)
            {
                MessageBox.Show("The maximum number of cups is 1000.");
                box.Text = $"{ctsCupCount}";
                return;
            }
            for (UInt16 ite = ctsCupCount; ite < newCount; ite++)
            {
                cups.Add(new Cup(ite));
            }
            ctsCupCount = newCount;
        }

        private void OnFilenameChange(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int idx = Grid.GetRow(box);
            cups[curCup].fileNames[idx] = box.Text;
        }

        private void OnTracknameChange(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.IsKeyboardFocused)
            {
                int idx = Grid.GetRow(box);
                cups[curCup].trackNames[idx] = box.Text;
                TextBlock ghostLabel = GhostGrid.Children.Cast<UIElement>().First(x => Grid.GetRow(x) == idx + 1 && Grid.GetColumn(x) == 0) as TextBlock;
                ghostLabel.Text = box.Text == "Name" ? $"Track {idx + 1}" : box.Text;
            }
        }

        private void OnAuthorChange(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            int idx = Grid.GetRow(box);
            cups[curCup].authorNames[idx] = box.Text;
        }

        private void OnSlotChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Grid.GetRow(box);
            cups[curCup].slots[idx] = idxToGameId[box.SelectedIndex];
        }

        private void OnMusicChange(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            int idx = Grid.GetRow(box);
            cups[curCup].musicSlots[idx] = idxToGameId[box.SelectedIndex];
        }

        private void OnLeftArrowClick(object sender, RoutedEventArgs e)
        {
            UpdateCurCup(-1);
        }
        private void OnRightArrowClick(object sender, RoutedEventArgs e)
        {
            UpdateCurCup(1);
        }
        private void UpdateCurCup(Int16 direction)
        {
            curCup = (UInt16)((curCup + ctsCupCount + direction) % ctsCupCount);
            if (curCup + 1 <= ctsCupCount)
            {
                Cup cup = cups[curCup];

                File1.Text = cup.fileNames[0];
                File2.Text = cup.fileNames[1];
                File3.Text = cup.fileNames[2];
                File4.Text = cup.fileNames[3];
                Name1.Text = cup.trackNames[0];
                Name2.Text = cup.trackNames[1];
                Name3.Text = cup.trackNames[2];
                Name4.Text = cup.trackNames[3];
                Author1.Text = cup.authorNames[0];
                Author2.Text = cup.authorNames[1];
                Author3.Text = cup.authorNames[2];
                Author4.Text = cup.authorNames[3];

                Ghost11.Text = UpdateExpert(cup, 0, 0);
                Ghost12.Text = UpdateExpert(cup, 0, 1);
                Ghost13.Text = UpdateExpert(cup, 0, 2);
                Ghost14.Text = UpdateExpert(cup, 0, 3);

                Ghost21.Text = UpdateExpert(cup, 1, 0);
                Ghost22.Text = UpdateExpert(cup, 1, 1);
                Ghost23.Text = UpdateExpert(cup, 1, 2);
                Ghost24.Text = UpdateExpert(cup, 1, 3);

                Ghost31.Text = UpdateExpert(cup, 2, 0);
                Ghost32.Text = UpdateExpert(cup, 2, 1);
                Ghost33.Text = UpdateExpert(cup, 2, 2);
                Ghost34.Text = UpdateExpert(cup, 2, 3);

                Ghost41.Text = UpdateExpert(cup, 3, 0);
                Ghost42.Text = UpdateExpert(cup, 3, 1);
                Ghost43.Text = UpdateExpert(cup, 3, 2);
                Ghost44.Text = UpdateExpert(cup, 3, 3);

                Slot1.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.slots[0])];
                Slot2.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.slots[1])];
                Slot3.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.slots[2])];
                Slot4.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.slots[3])];
                Music1.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.musicSlots[0])];
                Music2.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.musicSlots[1])];
                Music3.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.musicSlots[2])];
                Music4.SelectedValue = idxToAbbrev[Array.IndexOf(idxToGameId, cup.musicSlots[3])];
                CupIdLabel.Text = $"Cup {curCup + 1}";
            }
        }

        private void OnAlphabetizeClick(object sender, RoutedEventArgs e)
        {
            List<Cup> sortedCups = new List<Cup>(new Cup[cups.Count()]);
            for (UInt16 i = 0; i < cups.Count(); i++)
            {
                sortedCups[i] = new Cup(i);
            }

            string[] indexedArray = new string[ctsCupCount * 4];
            for (int idx = 0; idx < ctsCupCount; idx++)
            {
                Cup cup = cups[idx];
                for (int i = 0; i < 4; i++)
                {
                    indexedArray[cup.idx * 4 + i] = cup.trackNames[i];
                }
            }
            string[] sortedArray = new string[ctsCupCount * 4];
            Array.Copy(indexedArray, sortedArray, indexedArray.Length);
            Array.Sort(sortedArray);
            int cupIdx = 0;
            int trackIdx = 0;
            foreach (string s in sortedArray)
            {
                int idx = Array.IndexOf(indexedArray, s);
                int oldCupIdx = idx / 4;
                int oldTrackIdx = idx % 4;

                sortedCups[cupIdx].slots[trackIdx] = cups[oldCupIdx].slots[oldTrackIdx];
                sortedCups[cupIdx].musicSlots[trackIdx] = cups[oldCupIdx].musicSlots[oldTrackIdx];
                sortedCups[cupIdx].fileNames[trackIdx] = cups[oldCupIdx].fileNames[oldTrackIdx];
                sortedCups[cupIdx].trackNames[trackIdx] = cups[oldCupIdx].trackNames[oldTrackIdx];
                sortedCups[cupIdx].authorNames[trackIdx] = cups[oldCupIdx].authorNames[oldTrackIdx];
                sortedCups[cupIdx].expertFileNames[trackIdx, 0] = cups[oldCupIdx].expertFileNames[oldTrackIdx, 0];
                sortedCups[cupIdx].expertFileNames[trackIdx, 1] = cups[oldCupIdx].expertFileNames[oldTrackIdx, 1];
                sortedCups[cupIdx].expertFileNames[trackIdx, 2] = cups[oldCupIdx].expertFileNames[oldTrackIdx, 2];
                sortedCups[cupIdx].expertFileNames[trackIdx, 3] = cups[oldCupIdx].expertFileNames[oldTrackIdx, 3];
                trackIdx++;
                if (trackIdx == 4)
                {
                    trackIdx = 0;
                    cupIdx++;
                }
            }
            cups = sortedCups;
            UpdateCurCup(0);
            MessageBox.Show("Tracks have been sorted alphabetically.");
        }

        private string UpdateExpert(Cup cup, int row, int col)
        {
            if (cup.expertFileNames[row, col] == "") return "RKG File";
            else return cup.expertFileNames[row, col];
        }
        private void OnGhostChange(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.IsKeyboardFocused)
            {
                int row = Grid.GetRow(box) - 1;
                int col = Grid.GetColumn(box) - 1;
                string oldText = cups[curCup].expertFileNames[row, col];
                cups[curCup].expertFileNames[row, col] = box.Text;
                if (box.Text != "RKG File" && box.Text != "" && (oldText == "RKG File" || oldText == ""))
                {
                    trophyCount[col]++;
                }
                else if (trophyCount[col] > 0) trophyCount[col]--;
            }
        }
    }
}