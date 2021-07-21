using System;
using System.Collections.Generic;
using System.Linq;
using AElf.Contracts.MultiToken;
using AElf.CSharp.Core;
using Google.Protobuf.WellKnownTypes;

namespace AElf.Contracts.Lottery
{
    public partial class LotteryContract
    {
        public override OwnLottery Stake(Int64Value input)
        {
            Assert(input.Value > 0, "Invalid staking amount.");
            Assert(Context.CurrentBlockTime < State.StakingShutdownTimestamp.Value, "Activity already finished.");
            if (State.CurrentPeriodId.Value == 1)
            {
                Assert(Context.CurrentBlockTime >= State.StakingStartTimestamp.Value, "Activity not started yet.");
            }

            // Stake ELF Tokens.
            var ownLottery = State.OwnLotteryMap[Context.Sender] ?? new OwnLottery();
            var supposedLotteryAmount = CalculateSupposedLotteryAmount(ownLottery, input.Value);
            var newLotteryAmount = supposedLotteryAmount.Sub(ownLottery.LotteryCodeList.Count);
            Assert(newLotteryAmount >= 0, "Incorrect state.");
            if (newLotteryAmount == 0)
            {
                // Just update LotteryList.TotalStakingAmount
                ownLottery.TotalStakingAmount = ownLottery.TotalStakingAmount.Add(input.Value);
                return ownLottery;
            }

            var newLotteryCodeList = newLotteryAmount == 1
                ? new List<long> { State.CurrentLotteryCode.Value }
                : Enumerable.Range((int)State.CurrentLotteryCode.Value, newLotteryAmount).Select(i => (long)i).ToList();
            State.TokenContract.TransferFrom.Send(new TransferFromInput
            {
                From = Context.Sender,
                To = Context.Self,
                Amount = input.Value,
                Symbol = TokenSymbol,
                Memo = newLotteryAmount == 1
                    ? $"Got lottery with code {newLotteryCodeList.First()}"
                    : $"Got lotteries with code from {newLotteryCodeList.First()} to {newLotteryCodeList.Last()}"
            });

            foreach (var newLotteryCode in newLotteryCodeList)
            {
                var lottery = new Lottery
                {
                    LotteryCode = newLotteryCode,
                    IssueTimestamp = Context.CurrentBlockTime,
                    Owner = Context.Sender
                };

                ownLottery.LotteryCodeList.Add(newLotteryCode);

                // Update Lottery Map.
                State.LotteryMap[lottery.LotteryCode] = lottery;
            }

            State.CurrentLotteryCode.Value = State.CurrentLotteryCode.Value.Add(newLotteryAmount);

            // Update LotteryList Map.
            ownLottery.TotalStakingAmount = ownLottery.TotalStakingAmount.Add(newLotteryAmount);
            State.OwnLotteryMap[Context.Sender] = ownLottery;

            return ownLottery;
        }

        public override Empty Claim(Empty input)
        {
            var ownLottery = State.OwnLotteryMap[Context.Sender];
            var claimingAmount = 0L;
            foreach (var lotteryCode in ownLottery.LotteryCodeList)
            {
                var lottery = State.LotteryMap[lotteryCode];
                foreach (var awardId in lottery.AwardIdList)
                {
                    if (awardId <= lottery.LatestClaimedAwardId)
                    {
                        continue;
                    }

                    var award = State.AwardMap[awardId];
                    award.IsClaimed = true;
                    State.AwardMap[awardId] = award;

                    // Double check.
                    Assert(award.LotteryCode == lottery.LotteryCode,
                        $"Some wrong with the Award Id {awardId} and Lottery Code {lottery.LotteryCode}.");
                    claimingAmount = claimingAmount.Add(award.AwardAmount);

                    lottery.LatestClaimedAwardId = awardId;
                }

                State.LotteryMap[lotteryCode] = lottery;
            }

            Assert(ownLottery.TotalAwardAmount.Sub(ownLottery.ClaimedAwardAmount) == claimingAmount,
                $"Incorrect claiming award amount {claimingAmount}. OwnLottery: {ownLottery}");

            State.TokenContract.Transfer.Send(new TransferInput
            {
                To = Context.Sender,
                Symbol = TokenSymbol,
                Amount = claimingAmount,
                Memo = "Awards"
            });

            ownLottery.ClaimedAwardAmount = ownLottery.TotalAwardAmount;

            State.OwnLotteryMap[Context.Sender] = ownLottery;

            return new Empty();
        }

        public override Empty Redeem(Empty input)
        {
            Assert(Context.CurrentBlockTime >= State.RedeemTimestamp.Value,
                $"Cannot redeem before {State.RedeemTimestamp.Value}.");
            var ownLottery = State.OwnLotteryMap[Context.Sender];
            Assert(!ownLottery.IsRedeemed, "Already redeemed.");
            State.TokenContract.Transfer.Send(new TransferInput
            {
                To = Context.Sender,
                Symbol = TokenSymbol,
                Amount = ownLottery.TotalStakingAmount,
                Memo = "Redeem staked tokens."
            });

            ownLottery.IsRedeemed = true;
            State.OwnLotteryMap[Context.Sender] = ownLottery;

            return new Empty();
        }

        private int CalculateSupposedLotteryAmount(OwnLottery ownLottery, long additionalStakingAmount)
        {
            var totalStakingAmount = ownLottery.TotalStakingAmount.Add(additionalStakingAmount);
            var remainStakingAmount = totalStakingAmount;
            remainStakingAmount = remainStakingAmount.Sub(AmountOfElfToGetFirstLotteryCode);
            if (remainStakingAmount < 0)
            {
                return 0;
            }

            return Math.Min(MaximumLotteryCodeAmountForSingleAddress,
                (int)remainStakingAmount.Div(AmountOfElfToGetMoreLotteryCode)
                    .Add(1) // Add the first lottery code which cost AmountOfElfToGetFirstLotteryCode ELF tokens.
            );
        }
    }
}