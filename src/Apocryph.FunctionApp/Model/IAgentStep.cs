using System.Collections.Generic;

namespace Apocryph.FunctionApp.Model
{
    public interface IAgentStep
    {
        Hash Previous { get; set; }

        Hash PreviousValidatorSet { get; set; }

        List<ISigned<Commit>> PreviousCommits { get; set; }
    }
}