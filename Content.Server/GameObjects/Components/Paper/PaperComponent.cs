﻿using Content.Server.GameObjects.EntitySystems.Click;
using Content.Shared.GameObjects.Components;
using Content.Shared.Interfaces.GameObjects.Components;
using Robust.Server.GameObjects;
using Robust.Server.GameObjects.Components.UserInterface;
using Robust.Server.Interfaces.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Server.GameObjects.Components.Interactable
{
    [RegisterComponent]
    public class PaperComponent : SharedPaperComponent, IExamine, IInteractUsing, IUse
    {

        private BoundUserInterface _userInterface;
        private string _content;
        private PaperAction _mode;

        public override void Initialize()
        {
            base.Initialize();
            _userInterface = Owner.GetComponent<ServerUserInterfaceComponent>()
                .GetBoundUserInterface(PaperUiKey.Key);
            _userInterface.OnReceiveMessage += OnUiReceiveMessage;
            _content = "";
            _mode = PaperAction.Read;
            UpdateUserInterface();
        }
        private void UpdateUserInterface()
        {
            _userInterface.SetState(new PaperBoundUserInterfaceState(_content, _mode));
        }

        public void Examine(FormattedMessage message, bool inDetailsRange)
        {
            if (!inDetailsRange)
                return;

            message.AddMarkup(_content);
        }

        public bool UseEntity(UseEntityEventArgs eventArgs)
        {
            if (!eventArgs.User.TryGetComponent(out IActorComponent actor))
                return false;
            _mode = PaperAction.Read;
            UpdateUserInterface();
            _userInterface.Open(actor.playerSession);
            return true;
        }

        private void OnUiReceiveMessage(ServerBoundUserInterfaceMessage obj)
        {
            var msg = (PaperInputText) obj.Message;
            if (string.IsNullOrEmpty(msg.Text))
                return;

            _content += msg.Text + '\n';

            if (Owner.TryGetComponent(out SpriteComponent sprite))
            {
                sprite.LayerSetState(1, "paper_words");
            }

            UpdateUserInterface();
        }

        public bool InteractUsing(InteractUsingEventArgs eventArgs)
        {
            if (!eventArgs.Using.HasComponent<WriteComponent>())
                return false;
            if (!eventArgs.User.TryGetComponent(out IActorComponent actor))
                return false;

            _mode = PaperAction.Write;
            UpdateUserInterface();
            _userInterface.Open(actor.playerSession);
            return true;
        }
    }
}
