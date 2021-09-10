using System;
namespace TrueCraft.Client.Handlers
{
    /// <summary>
    /// Encapsulates the action to be taken when a TransactionStatusPacket is received.
    /// </summary>
    public class ActionConfirmation
    {
        private static int _nextActionNumber = 1;

        private readonly int _actionNumber;

        private Action _action;

        public static ActionConfirmation GetActionConfirmation(Action action)
        {
            ActionConfirmation rv = new ActionConfirmation(_nextActionNumber, action);
            _nextActionNumber++;
            if ((_nextActionNumber & 0xffff8000) != 0)
                _nextActionNumber = 1;

            return rv;
        }

        private ActionConfirmation(int actionNumber, Action action)
        {
            _actionNumber = actionNumber;
            _action = action;
        }

        public short ActionNumber { get => (short)_actionNumber; }

        public void TakeAction()
        {
            _action();
        }
    }
}
