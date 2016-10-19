using System;
using System.Drawing;
using System.Globalization;

namespace GuaranteedRate
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public Color FavoriteColor { get; set; }
        public DateTime DateOfBirth { get; set; }

        public override string ToString()
        {
            return string.Format("{0} , {1} , {2} , {3} , {4}",
                FirstName, LastName, Gender, FavoriteColor, DateOfBirth.ToString("M/d/yyyy", CultureInfo.InvariantCulture));
        }
    }
}
