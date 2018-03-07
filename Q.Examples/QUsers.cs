using System;

namespace Q.Examples
{
    public class QUsers
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? BirthDate { get; set; }        

        public string Key { get; set; }


        public override string ToString()
        {
            return $"{FirstName} {LastName} ({Key})";
        }
    }
}