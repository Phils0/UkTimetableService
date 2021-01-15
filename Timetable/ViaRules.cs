using System.Collections.Generic;

namespace Timetable
{
    public class ViaRules
    {
        private Dictionary<Location, List<ViaRule>> _rules = new Dictionary<Location, List<ViaRule>>();
        
        public void AddRule(ViaRule viaRule)
        {
            if (!_rules.TryGetValue(viaRule.Destination, out var destinationRules))
            {
                destinationRules = new List<ViaRule>();
                _rules.Add(viaRule.Destination, destinationRules);
            }

            // Want the rules with Location2 before those without so they get found first
            if (viaRule.HasLocation2)
            {
                destinationRules.Insert(0, viaRule);
            }
            else
            {
                destinationRules.Add(viaRule);
            }
        }
        
        public string GetViaText(ISchedule schedule)
        {
            //HACK Destination can be null when not in the Stations file.
            //TODO Fix, need better locations: ideally would be IDMS.
            if (_rules.TryGetValue(schedule.Destination?.Location ?? Location.NotSet, out var destinationRules))
            {
                foreach (var rule in destinationRules)
                {
                    if (rule.IsSatisfied(schedule))
                        return rule.Text;
                }
            }
            
            return string.Empty;
        }
    }
}