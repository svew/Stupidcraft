﻿using System;
using TrueCraft.Core.AI;
using TrueCraft.Core.Entities;
using TrueCraft.Core.Server;

namespace TrueCraft.Core.AI
{
    public class IdleState : IMobState
    {
        private DateTime Expiry { get; set; }
        private IMobState NextState { get; set; }
        
        public IdleState(IMobState nextState, DateTime? expiry = null)
        {
            NextState = nextState;
            if (expiry != null)
                Expiry = expiry.Value;
            else
                Expiry = DateTime.UtcNow.AddSeconds(MathHelper.Random.Next(5, 15));
        }

        public void Update(IMobEntity entity, IEntityManager manager)
        {
            if (DateTime.UtcNow >= Expiry)
                entity.CurrentState = NextState;
        }
    }
}
