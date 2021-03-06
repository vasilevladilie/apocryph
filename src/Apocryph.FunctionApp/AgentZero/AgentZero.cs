using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Apocryph.FunctionApp.Agent;
using Apocryph.FunctionApp.AgentZero.Messages;
using Apocryph.FunctionApp.AgentZero.Publications;
using Apocryph.FunctionApp.AgentZero.State;
using Apocryph.FunctionApp.Model;
using Microsoft.Azure.WebJobs;

namespace Apocryph.FunctionApp.AgentZero
{
    public static class AgentZero
    {
        public class State
        {
            public BalancesState Balances { get; set; }
            public StakesState Stakes { get; set; }
            public AgentsState Agents { get; set; }
        }

        public static void Run(IAgentContext<State> context, string sender, object message)
        {
            switch (message)
            {
                case TransferMessage transferMessage:
                    context.State.Balances.RemoveTokens(sender, transferMessage.Amount);
                    context.State.Balances.AddTokens(transferMessage.To, transferMessage.Amount);
                    context.MakePublication(new TransferPublication
                    {
                        From = sender,
                        To = transferMessage.To,
                        Amount = transferMessage.Amount,
                    });
                    break;

                case StakeMessage stakeMessage:
                    context.State.Balances.RemoveTokens(sender, stakeMessage.Amount);
                    context.State.Stakes.AddStake(sender, stakeMessage.To, stakeMessage.Amount);
                    context.MakePublication(new StakePublication
                    {
                        From = sender,
                        To = stakeMessage.To,
                        Amount = stakeMessage.Amount,
                    });
                    break;

                case UnstakeMessage unstakeMessage:
                    context.State.Stakes.RemoveStake(sender, unstakeMessage.From, unstakeMessage.Amount);
                    context.State.Balances.AddTokens(sender, unstakeMessage.Amount);
                    context.MakePublication(new StakePublication
                    {
                        From = sender,
                        To = unstakeMessage.From,
                        Amount = -unstakeMessage.Amount,
                    });
                    break;

                case RegisterAgentMessage registerAgentMessage:
                    context.State.Balances.RemoveTokens(sender, registerAgentMessage.InitialBalance);
                    context.State.Balances.AddTokens(registerAgentMessage.AgentId, registerAgentMessage.InitialBalance);
                    context.State.Agents.RegisterAgent(registerAgentMessage.AgentId);
                    context.MakePublication(new ValidatorSetPublication
                    {
                        AgentId = registerAgentMessage.AgentId,
                        Weights = context.State.Stakes.Amounts.ToDictionary(kv => kv.Key, kv => kv.Value.Values.Aggregate(BigInteger.Add)),
                    });
                    break;
            }

        }
    }
}