using BackEnd.Models;

namespace BackEnd.Builders
{
    public class AwardBuilder
    {
        private readonly Award _award = new Award();

        public AwardBuilder WithType(string awardType)
        {
            _award.Award_Type = awardType;
            return this;
        }

        public AwardBuilder WithGrantDate(DateTime grantDate)
        {
            _award.Grant_Date = grantDate;
            return this;
        }

        public AwardBuilder WithTotalUnits(int totalUnits)
        {
            _award.Total_Units = totalUnits;
            return this;
        }

        public AwardBuilder WithExercisePrice(double exercisePrice)
        {
            _award.Exercise_Price = exercisePrice;
            return this;
        }

        public AwardBuilder ForEmployee(int employeeEmpId)
        {
            _award.Employee_EmpId = employeeEmpId;
            return this;
        }

        public Award Build()
        {
            return _award;
        }
    }
}