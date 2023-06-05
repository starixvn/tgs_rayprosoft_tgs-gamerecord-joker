using Tgs.GameInterface.Joker.Protos;
using Tgs.GameRecord.JokerService.Core.Models;

namespace Tgs.GameRecord.JokerService.Core;

public interface IGameRecordUtensil
{
    public ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordInfo source,
        CancellationToken cancellationToken = default);
}
