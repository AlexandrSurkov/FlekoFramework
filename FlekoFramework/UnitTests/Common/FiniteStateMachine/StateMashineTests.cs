using Flekosoft.Common.Logging;
using Flekosoft.UnitTests.Common.Messaging;
using FlekoSoft.Common.FiniteStateMachine;
using FlekoSoft.Common.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flekosoft.UnitTests.Common.FiniteStateMachine
{
    [TestClass]
    public class StateMashineTests : MessageHandler
    {
        private readonly StateMashine<StateMashineTests> _stateMashine;
        private readonly TestState1 _state1 = new TestState1();
        private readonly TestState2 _state2 = new TestState2();
        private readonly GlobalState _globalState = new GlobalState();

        public StateMashineTests()
        {
            _stateMashine = new StateMashine<StateMashineTests>(this);
            Logger.Instance.LogerOutputs.Add(Logger.ConsoleOutput);
            _stateMashine.Dispose();
        }


        [TestMethod]
        public void StatesTests()
        {
            MessageDispatcher.Instance.RegisterHandler(this);

            //Устанавливаем первое состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _stateMashine.SetState(_state1);
            Assert.AreEqual(_state1, _stateMashine.CurrentState);
            Assert.IsTrue(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            //Выполняем первое состояние
            _state1.EnterExecuted = false;
            _stateMashine.Update(10);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsTrue(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            _state1.ExecuteExecuted = false;
            //Проверяем сообщение
            MessageDispatcher.Instance.DispatchMessage(new TestMessage(this, this));
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsTrue(_state1.MessageHandled);


            //Переключаемся на второе состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _stateMashine.SetState(_state2);
            Assert.AreEqual(_state2, _stateMashine.CurrentState);
            Assert.AreEqual(_state1, _stateMashine.PreviousState);
            Assert.IsTrue(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsTrue(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);

            //Выполняем второе состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _stateMashine.Update(10);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsTrue(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);

            //Проверяем сообщение
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(new TestMessage(this, this));
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsTrue(_state2.MessageHandled);
            Assert.IsFalse(_state1.MessageHandled);


            //Устанавливаем глобальное состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            _stateMashine.SetGlobalState(_globalState);
            Assert.AreEqual(_globalState, _stateMashine.GlobalState);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsTrue(_globalState.EnterExecuted);
            Assert.IsFalse(_globalState.ExecuteExecuted);
            Assert.IsFalse(_globalState.ExitExecuted);
            Assert.IsFalse(_globalState.MessageHandled);


            //Обновляем глобальное состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            _stateMashine.Update(10);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsTrue(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_globalState.EnterExecuted);
            Assert.IsTrue(_globalState.ExecuteExecuted);
            Assert.IsFalse(_globalState.ExitExecuted);
            Assert.IsFalse(_globalState.MessageHandled);


            //Меняем глобальное состояние
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            _stateMashine.SetGlobalState(_state1);
            Assert.AreEqual(_state1, _stateMashine.GlobalState);
            Assert.IsTrue(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_globalState.EnterExecuted);
            Assert.IsFalse(_globalState.ExecuteExecuted);
            Assert.IsTrue(_globalState.ExitExecuted);
            Assert.IsFalse(_globalState.MessageHandled);


            //Меняем глобальное состояние обратно
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            _stateMashine.SetGlobalState(_globalState);
            Assert.AreEqual(_globalState, _stateMashine.GlobalState);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsTrue(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsTrue(_globalState.EnterExecuted);
            Assert.IsFalse(_globalState.ExecuteExecuted);
            Assert.IsFalse(_globalState.ExitExecuted);
            Assert.IsFalse(_globalState.MessageHandled);

            //Проверяем событие для глобального состояния
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            MessageDispatcher.Instance.DispatchMessage(new TestMessage(this, this));
            Assert.AreEqual(_globalState, _stateMashine.GlobalState);
            Assert.IsFalse(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsFalse(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_globalState.EnterExecuted);
            Assert.IsFalse(_globalState.ExecuteExecuted);
            Assert.IsFalse(_globalState.ExitExecuted);
            Assert.IsTrue(_globalState.MessageHandled);


            //Проверяем возврат к предыдущему состоянию
            _state1.EnterExecuted = false;
            _state1.ExecuteExecuted = false;
            _state1.ExitExecuted = false;
            _state1.MessageHandled = false;
            _state2.EnterExecuted = false;
            _state2.ExecuteExecuted = false;
            _state2.ExitExecuted = false;
            _state2.MessageHandled = false;
            _globalState.EnterExecuted = false;
            _globalState.ExecuteExecuted = false;
            _globalState.ExitExecuted = false;
            _globalState.MessageHandled = false;
            _stateMashine.RevertToPreviousState();
            Assert.AreEqual(_state1, _stateMashine.CurrentState);
            Assert.AreEqual(_state2, _stateMashine.PreviousState);
            Assert.IsTrue(_state1.EnterExecuted);
            Assert.IsFalse(_state1.ExecuteExecuted);
            Assert.IsFalse(_state1.ExitExecuted);
            Assert.IsFalse(_state1.MessageHandled);
            Assert.IsFalse(_state2.EnterExecuted);
            Assert.IsFalse(_state2.ExecuteExecuted);
            Assert.IsTrue(_state2.ExitExecuted);
            Assert.IsFalse(_state2.MessageHandled);
            Assert.IsFalse(_globalState.EnterExecuted);
            Assert.IsFalse(_globalState.ExecuteExecuted);
            Assert.IsFalse(_globalState.ExitExecuted);
            Assert.IsFalse(_globalState.MessageHandled);



        }

        public override bool HandleMessage(Message message)
        {
            return _stateMashine.HandleMessage(message);
        }
    }
    class TestState1 : State<StateMashineTests>
    {
        public bool EnterExecuted { get; set; }
        public bool ExecuteExecuted { get; set; }
        public bool ExitExecuted { get; set; }
        public bool MessageHandled { get; set; }

        public override void Enter(StateMashineTests subject)
        {
            EnterExecuted = true;
        }

        public override void Execute(StateMashineTests subject, double timeDeltaMs)
        {
            ExecuteExecuted = true;
        }

        public override void Exit(StateMashineTests subject)
        {
            ExitExecuted = true;
        }

        public override bool OnMessage(StateMashineTests subject, Message message)
        {
            MessageHandled = true;
            return true;
        }
    }

    class TestState2 : TestState1
    {

    }

    class GlobalState : TestState1
    {

    }

}
