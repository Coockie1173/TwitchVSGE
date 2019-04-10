using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchChatReader
{
    class ParseCommands
    {

        //offsets
        UInt32 EmuOffset = 0x20000000;
        UInt32 TimerOffset = 0x800368B0;
        UInt32 StringBuffer = 0x80079C70;
        UInt32 StringBufferSec = 0x80079D04;
        UInt32 CurMenPag = 0x8002A8C0;
        UInt32 PlyHeight = 0x800552FC;
        UInt32 PlyYCam = 0x800550D4;
        UInt32 NearFog = 0x80044DCC;
        UInt32 FarFog = 0x80044DD0;
        UInt32 Explosions = 0x80036444;
        VAMemory vam = new VAMemory();

        float OrigCam;
        float OrigSize;

        public void Init()
        {
            //sets and reads base values
            vam.processName = "1964";
            OrigCam = vam.ReadFloat((IntPtr)(PlyYCam - 0x80000000 + EmuOffset));
            OrigSize = vam.ReadFloat((IntPtr)(PlyHeight - 0x80000000 + EmuOffset));
        }

        public void DispText(string Numbers)
        {
            //stop text from displaying
            vam.WriteByteArray((IntPtr)(TimerOffset - 0x80000000 + EmuOffset), new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            byte NewLineChar = Encoding.ASCII.GetBytes("*")[0];
            char[] TextToWrite = Numbers.ToCharArray();
            byte[] Write = Encoding.ASCII.GetBytes(TextToWrite);

            //big enough buffer, text shouldn't be longer than 101 anyways
            byte[] WriteArr = new byte[1024];
            Array.Copy(Write, 0, WriteArr, 0, Write.Length);
            byte[] Empty = new byte[100];

            //clear out text buffers
            vam.WriteByteArray((IntPtr)(StringBuffer - 0x80000000 + EmuOffset), Empty);
            vam.WriteByteArray((IntPtr)(StringBufferSec - 0x80000000 + EmuOffset), Empty);

            for (int i = 4; i < WriteArr.Length; i += 4)
            {
                //reverse 4 bytes
                byte[] Temp = new byte[4];
                Temp[3] = WriteArr[i - 4];
                Temp[2] = WriteArr[i - 3];
                Temp[1] = WriteArr[i - 2];
                Temp[0] = WriteArr[i - 1];

                //replace "*" with \n aka newline
                if (Temp.Contains(NewLineChar))
                {
                    for(int j = 0; j < 4; j++)
                    {
                        if (Temp[j] == NewLineChar)
                        {
                            Temp[j] = 0x0A;
                        }
                    }
                }

                //if there's nothing to write
                if (Temp[0] == 0 && Temp[3] == 0 && Temp[2] == 0 && Temp[1] == 0)
                {
                    break;
                }
                else
                {
                    //write our flipped array with \n in it
                    vam.WriteByteArray((IntPtr)(StringBuffer - 0x80000000 + EmuOffset + i - 4), Temp);
                }
            }


            //this is no joke the hackiest solution ever.
            //only reason I had to do this in the first place is due to the second buffer not being 4 byte alligned!
            //same setup as above with on exception for first run, to make it "byte alligned"
            int Index = 0;
            while (Index < WriteArr.Length)
            {
                if (Index != 0)
                {
                    byte[] Temp = new byte[4];
                    Temp[3] = WriteArr[Index - 4];
                    Temp[2] = WriteArr[Index - 3];
                    Temp[1] = WriteArr[Index - 2];
                    Temp[0] = WriteArr[Index - 1];

                    if (Temp.Contains(NewLineChar))
                    {
                        for (int j = 0; j < 4; j++)
                        {
                            if (Temp[j] == NewLineChar)
                            {
                                Temp[j] = 0x0A;
                            }
                        }
                    }

                    if (Temp[0] == 0 && Temp[3] == 0 && Temp[2] == 0 && Temp[1] == 0)
                    {
                        break;
                    }
                    else
                    {
                        vam.WriteByteArray((IntPtr)(StringBufferSec - 0x80000000 + EmuOffset + Index - 1), Temp);
                    }
                    Index += 4;
                }
                else
                {
                    byte[] Temp = new byte[4];
                    Temp[0] = WriteArr[Index];
                    Temp[3] = 0;
                    Temp[2] = 0;
                    Temp[1] = 0;
                    vam.WriteByteArray((IntPtr)(StringBufferSec - 0x80000000 + EmuOffset + Index), Temp);
                    Index += 5;
                }
                
            }

            //vam.WriteByteArray((IntPtr)(StringBufferSec - 0x80000000 + EmuOffset - 1), Output.ToArray());
            vam.WriteByteArray((IntPtr)(TimerOffset - 0x80000000 + EmuOffset), new byte[] { 0xC0, 0x03, 0x00, 0x00 });
        }


        public string GetCommName(int Num)
        {
            //simple case switch to convert our random number into a command name
            string RS = "";
            switch(Num)
            {
                case (0):
                    {
                        RS = "Strip ammo";
                        break;
                    }
                case (1):
                    {
                        RS = "Give ammo";
                        break;
                    }
                case (2):
                    {
                        RS = "Terrible View";
                        break;
                    }
                case (3):
                    {
                        RS = "Lock camera y";
                        break;
                    }
                case (4):
                    {
                        //RS = "Launch Player"; -- took this out
                        RS = "Strong Guards";
                        break;
                    }
                case (5):
                    {
                        RS = "Tiny bond";
                        break;
                    }
                case (6):
                    {
                        RS = "Steal Health";
                        break;
                    }
                case (7):
                    {
                        RS = "Explode";
                        break;
                    }
                case (8):
                    {
                        RS = "Wonky view";
                        break;
                    }
                case (9):
                    {
                        RS = "Heal Player";
                        break;
                    }
                case (10):
                    {
                        RS = "Fast Camera";
                        break;
                    }
            }

            return RS;
        }

        public void ExecCom(int CommNum)
        {
            //switch case to exectute command
            UInt32 BondPos = GetPly();
            switch (CommNum)
            {
                case (0):
                    {
                        DispText("Stripping ammo*");
                        for (int i = 0; i < 116; i += 4)
                        {
                            vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x1130 + i), 0);
                        }                      

                        break;
                    }
                case (1):
                    {
                        DispText("Giving ammo*");
                        for (int i = 0; i < 116; i += 4)
                        {
                            vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x1130 + i), 5000);
                        }
                        break;
                    }
                case (2):
                    {
                        DispText("Making the view terrible...*");
                        vam.WriteInt32((IntPtr)(NearFog - 0x80000000 + EmuOffset), 0xE3);
                        vam.WriteInt32((IntPtr)(FarFog - 0x80000000 + EmuOffset), 0x3E8);
                        break;
                    }
                case (3):
                    {
                        DispText("Camera locked*");
                        vam.WriteFloat((IntPtr)(PlyYCam - 0x80000000 + EmuOffset), 0f);
                        break;
                    }
                case (4):
                    {
                        /*DispText("Launching!*");
                        float CurY = vam.ReadFloat((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0xB));
                        vam.WriteFloat((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x000D2FD0 - 0x000D2FFC +0x9C), CurY + 500f);*/
                        //breaks in facility
                        DispText("Making the guards stronger!*");
                        int AmmGuards = vam.ReadInt32((IntPtr)(0x8002CC68 - 0x80000000 + EmuOffset));
                        UInt32 GuardOffset = vam.ReadUInt32((IntPtr)(0x8002CC64 - 0x80000000 + EmuOffset));
                        for (int i = 0; i < AmmGuards; i++)
                        {
                            float OrigHealth = vam.ReadFloat((IntPtr)(GuardOffset - 0x80000000 + EmuOffset + 0x1DC * i));
                            vam.WriteFloat((IntPtr)(GuardOffset - 0x80000000 + EmuOffset + 0x1DC * i + 0x100), OrigHealth * 2);
                        }
                        break;
                    }
                case (5):
                    {
                        DispText("Shrinking!*");
                        vam.WriteFloat((IntPtr)(PlyHeight - 0x80000000 + EmuOffset), 0.02f);
                        break;
                    }
                case (6):
                    {
                        DispText("Stealing!*");
                        float OldHealth = vam.ReadFloat((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x000D303C - 0x000D2FFC + 0x9C));
                        vam.WriteFloat((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x000D303C - 0x000D2FFC + 0x9C), OldHealth / 2);
                        vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x2A00), 00);
                        vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x2A00), 0x3C * 5);
                        break;
                    }
                case (7):
                    {
                        DispText("Baboom!*");
                        vam.WriteBoolean((IntPtr)(Explosions - 0x80000000 + EmuOffset), true);
                        break;
                    }
                case (8):
                    {
                        DispText("Wonkenizing!*");
                        vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x000D4124 - 0x000D2FFC + 0x9C), 0x800000);
                        break;
                    }
                case (9):
                    {
                        DispText("Healing!*");
                        vam.WriteFloat((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x000D303C - 0x000D2FFC + 0x9C), 1);
                        vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x2A00), 00);
                        vam.WriteInt32((IntPtr)(BondPos - 0x80000000 + EmuOffset + 0x2A00), 0x3C * 5);
                        break;
                    }
                case (10):
                    {
                        DispText("Speeding up!*");
                        vam.WriteFloat((IntPtr)(PlyYCam - 0x80000000 + EmuOffset), 1f);
                        break;
                    }
            }
        }

        public void RemoveStatEffects()
        {
            //to clear out stuff like tiny bond, explosions and fast camera
            vam.WriteFloat((IntPtr)(PlyYCam - 0x80000000 + EmuOffset), OrigCam);
            vam.WriteFloat((IntPtr)(PlyHeight - 0x80000000 + EmuOffset), OrigSize);
            vam.WriteBoolean((IntPtr)(Explosions - 0x80000000 + EmuOffset), false);
        }

        private UInt32 GetPly()
        {
            //returns player pointer read from 8007A0B0
            return vam.ReadUInt32((IntPtr)(0x8007A0B0 - 0x80000000 + EmuOffset));
        }

        public bool CheckInStage()
        {
            //just checks if you're currently within a stage or not
            if (vam.ReadInt32((IntPtr)(CurMenPag - 0x80000000 + EmuOffset)) == 0x0B)
            {
                return true;    
            }
            return false;
        }
    }
}
