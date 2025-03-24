namespace SatelliteSoft
{
    partial class Customers
    {
        public override string ToString()
        {
            return name + " - " + contact_person + " (" + contact_info + ")";
        }
    }

    partial class Employees
    {
        public override string ToString()
        {
            return name + " - " + position;
        }
    }

    partial class Projects
    {
        public override string ToString()
        {
            return name;
        }
    }
}
