using System;
using System.Collections.Generic;
using Bot;
using MessagePipe;
using Messages;
using UnityEngine;
using UnityEngine.AI;

namespace StateMachine
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class StateMachineContext
    {
        public Nullable<AggroTarget> aggroTarget = null;
        public float DeltaTime;

        public NavMeshAgent agent;

        public Transform goal;
    }
}