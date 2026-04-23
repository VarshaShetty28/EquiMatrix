using BackEnd.Models;

namespace BackEnd.Builders
{
    public class VestingScheduleBuilder
    {
        private int _years = 5;
        private string _status = "Pending";

        public VestingScheduleBuilder WithYears(int years)
        {
            _years = years;
            return this;
        }

        public VestingScheduleBuilder WithStatus(string status)
        {
            _status = status;
            return this;
        }

        public List<VestingSchedule> BuildForAward(Award award)
        {
            var schedules = new List<VestingSchedule>();
            int unitsPerYear = award.Total_Units / _years;
            int cumulative = 0;

            for (int i = 1; i <= _years; i++)
            {
                cumulative += unitsPerYear;

                schedules.Add(new VestingSchedule
                {
                    Awards_AwardId = award.AwardId,
                    Employee_EmpId = award.Employee_EmpId,
                    Vesting_Date = award.Grant_Date.AddYears(i),
                    Units_Vested = unitsPerYear,
                    Cumulative_Vested = cumulative,
                    Status = _status
                });
            }

            return schedules;
        }
    }
}