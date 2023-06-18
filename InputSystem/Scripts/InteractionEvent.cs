namespace UTools.Input
{
    /// <summary>
    /// The basic interaction event.
    /// </summary>
    public class InteractionEvent
    {
        private static ulong m_Counter;

        private readonly ulong m_Id;

        public InteractionEvent() => m_Id = ++m_Counter;

        public override int GetHashCode() => m_Id.GetHashCode();
    }
}