using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    public class ProgramSettings
    {
        public TestConditions TestSettings { get; set; } = new TestConditions();
        public EngineParameters GAsettings { get; set; } = new EngineParameters();
    }
}
