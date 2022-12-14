using System.Reflection;
using System.Windows.Forms;

namespace ShareInvest.Communication
{
    public static class FindByNameUtil
    {
        public static T FindByName<T>(this object targetClass, string name) where T : class
        {
            return targetClass.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(targetClass) as T;
        }
        public static T FindByName<T>(this string name, object targetClass) where T : class
        {
            return targetClass.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(targetClass) as T;
        }
        public static NumericUpDown NumericUpDown(string name, object targetClass)
        {
            return FindByName<NumericUpDown>(targetClass, name);
        }
        public static NumericUpDown Is_NumericUpDown_From(this string name, object targetClass)
        {
            return FindByName<NumericUpDown>(targetClass, name);
        }
        public static Button Button(string name, object targetClass)
        {
            return FindByName<Button>(targetClass, name);
        }
        public static Button Is_Button_Form(this string name, object targetClass)
        {
            return FindByName<Button>(targetClass, name);
        }
        public static RadioButton RadioButton(string name, object targetClass)
        {
            return FindByName<RadioButton>(targetClass, name);
        }
        public static RadioButton Is_RadioButton_Form(this string name, object targetClass)
        {
            return FindByName<RadioButton>(targetClass, name);
        }
        public static Label Label(string name, object targetClass)
        {
            return FindByName<Label>(targetClass, name);
        }
        public static Label Is_Label_From(this string name, object targetClass)
        {
            return FindByName<Label>(targetClass, name);
        }
        public static TextBox TextBox(string name, object targetClass)
        {
            return FindByName<TextBox>(targetClass, name);
        }
        public static TextBox Is_TextBox_From(this string name, object targetClass)
        {
            return FindByName<TextBox>(targetClass, name);
        }
    }
}