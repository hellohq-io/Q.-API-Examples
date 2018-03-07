using System;

namespace Q.Examples
{
    public class DefaultResponseObject
    {
        /// <summary>
        /// The id of the object.
        /// </summary>
        /// <returns></returns>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        /// <returns></returns>
        public string Name { get; set; }


        public override string ToString()
        {
            return $"{Name}";
        }
    }
}