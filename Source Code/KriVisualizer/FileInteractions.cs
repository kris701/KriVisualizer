using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KriVisualizer
{
    public class FI : FIVItems
    {
        struct ItemPoint
        {
            public int X;
            public int Y;

            public ItemPoint(int _X, int _Y)
            {
                X = _X;
                Y = _Y;
            }
        }

        public struct CategoryData
        {
            public string Name;
            public int Offset;

            public CategoryData(string _Name, int _Offset)
            {
                Name = _Name;
                Offset = _Offset;
            }
        }

        public List<ValueCategory> ItemStack = new List<ValueCategory>();

        public void LoadFile(string FileLoc)
        {
            try
            {
                if (System.IO.File.Exists(FileLoc))
                {
                    string[] lines = System.IO.File.ReadAllLines(FileLoc);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("( ["))
                        {
                            ValueCategory NewCat = new ValueCategory();
                            NewCat.Name = lines[i].Replace(lines[i].Substring(0, lines[i].IndexOf(" : ") + 3),"").Replace(" ) { ", "");
                            NewCat.ValueStack = new List<FIItems>();
                            ItemStack.Add(NewCat);

                            int InnerI = i + 1;
                            while(lines[InnerI] != "}")
                            {
                                string ValueType = lines[InnerI].Replace("     ( ","").Substring(0, lines[InnerI].Replace("     ( ", "").IndexOf(" ) "));
                                if (ValueType == ValueTypes[(int)ValueTypesEnum.STR])
                                {
                                    StringValue NewItem = new StringValue(
                                        lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").Substring(0, lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").IndexOf(" = ")), "");

                                    if (CutStringTillData(lines[InnerI]).StartsWith("R["))
                                    {
                                        NewItem.Value = GetValueFromDataPoint(CutStringTillData(lines[InnerI]));
                                    }
                                    else
                                    {
                                        NewItem.Value = CutStringTillData(lines[InnerI]);
                                    }
                                    NewCat.ValueStack.Add(NewItem);
                                }
                                if (ValueType == ValueTypes[(int)ValueTypesEnum.INT])
                                {
                                    IntValue NewItem = new IntValue(
                                        lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").Substring(0, lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").IndexOf(" = ")), 0);

                                    if (CutStringTillData(lines[InnerI]).StartsWith("R["))
                                    {
                                        NewItem.Value = Int32.Parse(GetValueFromDataPoint(CutStringTillData(lines[InnerI])));
                                    }
                                    else
                                    {
                                        NewItem.Value = Int32.Parse(CutStringTillData(lines[InnerI]));
                                    }
                                    NewCat.ValueStack.Add(NewItem);
                                }
                                if (ValueType == ValueTypes[(int)ValueTypesEnum.BOL])
                                {
                                    BoolValue NewItem = new BoolValue(
                                        lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").Substring(0, lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").IndexOf(" = ")), false);

                                    if (CutStringTillData(lines[InnerI]).StartsWith("R["))
                                    {
                                        NewItem.Value = Convert.ToBoolean(GetValueFromDataPoint(CutStringTillData(lines[InnerI])));
                                    }
                                    else
                                    {
                                        NewItem.Value = Convert.ToBoolean(CutStringTillData(lines[InnerI]));
                                    }
                                    NewCat.ValueStack.Add(NewItem);
                                }
                                if (ValueType == ValueTypes[(int)ValueTypesEnum.BRS])
                                {
                                    BrushValue NewItem = new BrushValue(
                                        lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").Substring(0, lines[InnerI].Replace(lines[InnerI].Substring(0, lines[InnerI].IndexOf(":") + 2), "").IndexOf(" = ")), System.Windows.Media.Brushes.White);

                                    if (CutStringTillData(lines[InnerI]).StartsWith("R["))
                                    {
                                        NewItem.Value = (System.Windows.Media.SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFrom(GetValueFromDataPoint(CutStringTillData(lines[InnerI]))));
                                    }
                                    else
                                    {
                                        NewItem.Value = (System.Windows.Media.SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFrom(CutStringTillData(lines[InnerI])));
                                    }
                                    NewCat.ValueStack.Add(NewItem);
                                }
                                InnerI++;
                            }
                            i = InnerI;
                        }
                    }
                }
            }
            catch { }
        }

        public void SaveFile(string FileLoc)
        {
            try
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileLoc))
                {
                    for (int i = 0; i < ItemStack.Count; i++)
                    {
                        file.WriteLine("( [" + i + "] : " + ItemStack[i].Name + " ) { ");
                        for (int j = 0; j < ItemStack[i].ValueStack.Count; j++)
                        {
                            file.WriteLine("     ( " + ItemStack[i].ValueStack[j].GetValueType() + " ) " + " [" + j + "] : " + ItemStack[i].ValueStack[j].GetName() + " = " + FindAnyRefrence(ItemStack[i].ValueStack[j], i, j));
                        }
                        file.WriteLine("}");
                    }
                }
            }
            catch
            {

            }
        }

        string FindAnyRefrence(FIItems Item, int SenderI, int SenderJ)
        {
            if (Item.GetValueLength() > 6)
            {
                for (int i = 0; i <= SenderI; i++)
                {
                    for (int j = 0; j < ItemStack[i].ValueStack.Count; j++)
                    {
                        if (SenderI == i && j > SenderJ)
                            break;

                        if (!(j == SenderJ && SenderI == i))
                            if (ItemStack[i].ValueStack[j].GetValue() == Item.GetValue())
                                return "R[" + i + "." + j + "]";
                    }
                }
            }
            return Item.GetValue();
        }

        string GetValueFromDataPoint(string DataPoint)
        {
            string[] CutData = DataPoint.Replace("R[", "").Replace("]","").Split('.');
            int IVal = Int32.Parse(CutData[0]);
            int JVal = Int32.Parse(CutData[1]);
            if (IVal < ItemStack.Count)
            {
                if (JVal < ItemStack[IVal].ValueStack.Count)
                {
                    return ItemStack[IVal].ValueStack[JVal].GetValue();
                }
            }
            return "ERR";
        }

        string CutStringTillData(string Input)
        {
            return Input.Substring(Input.LastIndexOf(" = ") + 3);
        }

        public int FindtItemInItemstack_INT(CategoryData CatData, string Name)
        {
            ItemPoint MomentPoint = FindIndexOfCategory(CatData.Name, Name, CatData.Offset);
            if (MomentPoint.X != -1)
                if (MomentPoint.Y!= -1)
                    return Int32.Parse(ItemStack[MomentPoint.X].ValueStack[MomentPoint.Y].GetValue());
            return 0;
        }

        public string FindtItemInItemstack_STR(CategoryData CatData, string Name)
        {
            ItemPoint MomentPoint = FindIndexOfCategory(CatData.Name, Name, CatData.Offset);
            if (MomentPoint.X != -1)
                if (MomentPoint.Y != -1)
                    return ItemStack[MomentPoint.X].ValueStack[MomentPoint.Y].GetValue();
            return "";
        }

        public bool FindtItemInItemstack_BOL(CategoryData CatData, string Name)
        {
            ItemPoint MomentPoint = FindIndexOfCategory(CatData.Name, Name, CatData.Offset);
            if (MomentPoint.X != -1)
                if (MomentPoint.Y != -1)
                    return Convert.ToBoolean(ItemStack[MomentPoint.X].ValueStack[MomentPoint.Y].GetValue());
            return false;
        }

        public System.Windows.Media.Brush FindtItemInItemstack_BRS(CategoryData CatData, string Name)
        {
            ItemPoint MomentPoint = FindIndexOfCategory(CatData.Name, Name, CatData.Offset);
            if (MomentPoint.X != -1)
                if (MomentPoint.Y != -1)
                    return (System.Windows.Media.SolidColorBrush)(new System.Windows.Media.BrushConverter().ConvertFrom(ItemStack[MomentPoint.X].ValueStack[MomentPoint.Y].GetValue()));
            return System.Windows.Media.Brushes.White;
        }

        public bool IsAnyMoreInCat(CategoryData CatData)
        {
            int InnerCount = 0;
            for (int i = 0; i < ItemStack.Count; i++)
            {
                if (ItemStack[i].Name == CatData.Name)
                {
                    if (InnerCount >= CatData.Offset)
                        return true;
                    InnerCount++;
                }
            }
            return false;
        }

        private ItemPoint FindIndexOfCategory(string Cat, string Name, int Offset)
        {
            int InnerCount = 0;
            for (int i = 0; i < ItemStack.Count; i++)
            {
                if (ItemStack[i].Name == Cat)
                {
                    if (InnerCount >= Offset)
                    {
                        for (int j = 0; j < ItemStack[i].ValueStack.Count; j++)
                        {
                            if (ItemStack[i].ValueStack[j].GetName() == Name)
                            {
                                return new ItemPoint(i, j);
                            }
                        }
                    }
                    InnerCount++;
                }
            }
            return new ItemPoint(-1, -1);
        }
    }



    public class FIVItems
    {
        public static readonly string[] ValueTypes = { "STR", "INT", "BOL", "BRS" };
        public enum ValueTypesEnum { STR, INT, BOL, BRS };

        public interface FIItems
        {
            int GetValueLength();
            string GetValue();
            string GetName();
            string GetValueType();
        }

        public class StringValue : FIItems
        {
            public ValueTypesEnum ValueType;
            public string Name;
            public string Value;

            public StringValue(string _Name, string _Value)
            {
                ValueType = ValueTypesEnum.STR;
                Name = _Name;
                Value = _Value;
            }

            int FIItems.GetValueLength()
            {
                return Value.Length;
            }

            string FIItems.GetValue()
            {
                return Value;
            }

            string FIItems.GetName()
            {
                return Name;
            }

            string FIItems.GetValueType()
            {
                return ValueTypes[(int)ValueType];
            }
        }

        public class IntValue : FIItems
        {
            public ValueTypesEnum ValueType;
            public string Name;
            public int Value;

            public IntValue(string _Name, int _Value)
            {
                ValueType = ValueTypesEnum.INT;
                Name = _Name;
                Value = _Value;
            }

            int FIItems.GetValueLength()
            {
                if (Value == 0)
                    return 1;
                return (int)Math.Floor(Math.Log10(Value)) + 1;
            }

            string FIItems.GetValue()
            {
                return Value.ToString();
            }

            string FIItems.GetName()
            {
                return Name;
            }
            string FIItems.GetValueType()
            {
                return ValueTypes[(int)ValueType];
            }
        }

        public class BoolValue : FIItems
        {
            public ValueTypesEnum ValueType;
            public string Name;
            public bool Value;

            public BoolValue(string _Name, bool _Value)
            {
                ValueType = ValueTypesEnum.BOL;
                Name = _Name;
                Value = _Value;
            }

            int FIItems.GetValueLength()
            {
                if (Value == true)
                    return 4;
                return 5;
            }

            string FIItems.GetValue()
            {
                return Value.ToString();
            }

            string FIItems.GetName()
            {
                return Name;
            }
            string FIItems.GetValueType()
            {
                return ValueTypes[(int)ValueType];
            }
        }

        public class BrushValue : FIItems
        {
            public ValueTypesEnum ValueType;
            public string Name;
            public System.Windows.Media.Brush Value;

            public BrushValue(string _Name, System.Windows.Media.Brush _Value)
            {
                ValueType = ValueTypesEnum.BRS;
                Name = _Name;
                Value = _Value;
            }

            int FIItems.GetValueLength()
            {
                return ((System.Windows.Media.SolidColorBrush)Value).Color.ToString().Length;
            }

            string FIItems.GetValue()
            {
                return ((System.Windows.Media.SolidColorBrush)Value).Color.ToString();
            }

            string FIItems.GetName()
            {
                return Name;
            }
            string FIItems.GetValueType()
            {
                return ValueTypes[(int)ValueType];
            }
        }

        public struct ValueCategory
        {
            public string Name;
            public List<FIItems> ValueStack;

            public ValueCategory(string _Name, List<FIItems> _ItemStack)
            {
                Name = _Name;
                ValueStack = _ItemStack;
            }
        }
    }
}
