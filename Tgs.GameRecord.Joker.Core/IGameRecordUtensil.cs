using Tgs.GameInterface.Joker.Protos;
using Tgs.GameRecord.Joker.Core.Models;

namespace Tgs.GameRecord.Joker.Core;

public interface IGameRecordUtensil
{
    public ValueTask<RecordInfo> SettlementConvertRecordAsync(
        GameRecordInfo source,
        CancellationToken cancellationToken = default);
}
