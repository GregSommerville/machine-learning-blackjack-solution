namespace BlackjackStrategy.Models
{
    public class ProgramSettings
    {
        public TestConditions TestSettings { get; set; } = new TestConditions();
        public EngineParameters GAsettings { get; set; } = new EngineParameters();
    }
}
