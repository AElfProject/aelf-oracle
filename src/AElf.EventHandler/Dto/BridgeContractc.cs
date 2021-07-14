// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: bridge_contract.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace AElf.Contracts.Bridge {

  #region Events
  public partial class SwapRatioChanged : aelf::IEvent<SwapRatioChanged>
  {
    public global::System.Collections.Generic.IEnumerable<SwapRatioChanged> GetIndexed()
    {
      return new List<SwapRatioChanged>
      {
      };
    }

    public SwapRatioChanged GetNonIndexed()
    {
      return new SwapRatioChanged
      {
        SwapId = SwapId,
        NewSwapRatio = NewSwapRatio,
        TargetTokenSymbol = TargetTokenSymbol,
      };
    }
  }

  public partial class TokenSwapped : aelf::IEvent<TokenSwapped>
  {
    public global::System.Collections.Generic.IEnumerable<TokenSwapped> GetIndexed()
    {
      return new List<TokenSwapped>
      {
      };
    }

    public TokenSwapped GetNonIndexed()
    {
      return new TokenSwapped
      {
        Address = Address,
        Amount = Amount,
        Symbol = Symbol,
      };
    }
  }

  public partial class SwapPairAdded : aelf::IEvent<SwapPairAdded>
  {
    public global::System.Collections.Generic.IEnumerable<SwapPairAdded> GetIndexed()
    {
      return new List<SwapPairAdded>
      {
      };
    }

    public SwapPairAdded GetNonIndexed()
    {
      return new SwapPairAdded
      {
        SwapId = SwapId,
      };
    }
  }

  #endregion
  public static partial class BridgeContractContainer
  {
    static readonly string __ServiceName = "BridgeContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::AElf.Standards.ACS13.AggregateInput> __Marshaller_acs13_AggregateInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Standards.ACS13.AggregateInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.StringValue> __Marshaller_google_protobuf_StringValue = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.StringValue.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Int64Value> __Marshaller_google_protobuf_Int64Value = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Int64Value.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Hash> __Marshaller_aelf_Hash = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Hash.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListInput> __Marshaller_GetReceiptHashListInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListOutput> __Marshaller_GetReceiptHashListOutput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListOutput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.InitializeInput> __Marshaller_InitializeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.InitializeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.CreateSwapInput> __Marshaller_CreateSwapInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.CreateSwapInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.SwapTokenInput> __Marshaller_SwapTokenInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.SwapTokenInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.ChangeSwapRatioInput> __Marshaller_ChangeSwapRatioInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.ChangeSwapRatioInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.DepositInput> __Marshaller_DepositInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.DepositInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.WithdrawInput> __Marshaller_WithdrawInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.WithdrawInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.UpdateMerkleTreeInput> __Marshaller_UpdateMerkleTreeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.UpdateMerkleTreeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::CallbackInput> __Marshaller_CallbackInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::CallbackInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.SwapInfo> __Marshaller_SwapInfo = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.SwapInfo.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.GetSwapPairInput> __Marshaller_GetSwapPairInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.GetSwapPairInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.SwapPair> __Marshaller_SwapPair = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.SwapPair.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.GetSwapAmountsInput> __Marshaller_GetSwapAmountsInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.GetSwapAmountsInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Contracts.Bridge.SwapAmounts> __Marshaller_SwapAmounts = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Contracts.Bridge.SwapAmounts.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::AElf.Standards.ACS13.AggregateInput, global::Google.Protobuf.WellKnownTypes.StringValue> __Method_Aggregate = new aelf::Method<global::AElf.Standards.ACS13.AggregateInput, global::Google.Protobuf.WellKnownTypes.StringValue>(
        aelf::MethodType.Action,
        __ServiceName,
        "Aggregate",
        __Marshaller_acs13_AggregateInput,
        __Marshaller_google_protobuf_StringValue);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Int64Value> __Method_GetReceiptCount = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::Google.Protobuf.WellKnownTypes.Int64Value>(
        aelf::MethodType.View,
        __ServiceName,
        "GetReceiptCount",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_google_protobuf_Int64Value);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::AElf.Types.Hash> __Method_GetReceiptHash = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Int64Value, global::AElf.Types.Hash>(
        aelf::MethodType.View,
        __ServiceName,
        "GetReceiptHash",
        __Marshaller_google_protobuf_Int64Value,
        __Marshaller_aelf_Hash);

    static readonly aelf::Method<global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListInput, global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListOutput> __Method_GetReceiptHashList = new aelf::Method<global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListInput, global::AElf.Contracts.ReceiptMakerContract.GetReceiptHashListOutput>(
        aelf::MethodType.View,
        __ServiceName,
        "GetReceiptHashList",
        __Marshaller_GetReceiptHashListInput,
        __Marshaller_GetReceiptHashListOutput);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::AElf.Contracts.Bridge.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_InitializeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.CreateSwapInput, global::AElf.Types.Hash> __Method_CreateSwap = new aelf::Method<global::AElf.Contracts.Bridge.CreateSwapInput, global::AElf.Types.Hash>(
        aelf::MethodType.Action,
        __ServiceName,
        "CreateSwap",
        __Marshaller_CreateSwapInput,
        __Marshaller_aelf_Hash);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.SwapTokenInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SwapToken = new aelf::Method<global::AElf.Contracts.Bridge.SwapTokenInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SwapToken",
        __Marshaller_SwapTokenInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.ChangeSwapRatioInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_ChangeSwapRatio = new aelf::Method<global::AElf.Contracts.Bridge.ChangeSwapRatioInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "ChangeSwapRatio",
        __Marshaller_ChangeSwapRatioInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.DepositInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Deposit = new aelf::Method<global::AElf.Contracts.Bridge.DepositInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Deposit",
        __Marshaller_DepositInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.WithdrawInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Withdraw = new aelf::Method<global::AElf.Contracts.Bridge.WithdrawInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Withdraw",
        __Marshaller_WithdrawInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.UpdateMerkleTreeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_UpdateMerkleTree = new aelf::Method<global::AElf.Contracts.Bridge.UpdateMerkleTreeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "UpdateMerkleTree",
        __Marshaller_UpdateMerkleTreeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::CallbackInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_RecordReceiptHash = new aelf::Method<global::CallbackInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "RecordReceiptHash",
        __Marshaller_CallbackInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Hash, global::AElf.Contracts.Bridge.SwapInfo> __Method_GetSwapInfo = new aelf::Method<global::AElf.Types.Hash, global::AElf.Contracts.Bridge.SwapInfo>(
        aelf::MethodType.View,
        __ServiceName,
        "GetSwapInfo",
        __Marshaller_aelf_Hash,
        __Marshaller_SwapInfo);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.GetSwapPairInput, global::AElf.Contracts.Bridge.SwapPair> __Method_GetSwapPair = new aelf::Method<global::AElf.Contracts.Bridge.GetSwapPairInput, global::AElf.Contracts.Bridge.SwapPair>(
        aelf::MethodType.View,
        __ServiceName,
        "GetSwapPair",
        __Marshaller_GetSwapPairInput,
        __Marshaller_SwapPair);

    static readonly aelf::Method<global::AElf.Contracts.Bridge.GetSwapAmountsInput, global::AElf.Contracts.Bridge.SwapAmounts> __Method_GetSwapAmounts = new aelf::Method<global::AElf.Contracts.Bridge.GetSwapAmountsInput, global::AElf.Contracts.Bridge.SwapAmounts>(
        aelf::MethodType.View,
        __ServiceName,
        "GetSwapAmounts",
        __Marshaller_GetSwapAmountsInput,
        __Marshaller_SwapAmounts);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::AElf.Contracts.Bridge.BridgeContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS13.Acs13Reflection.Descriptor.Services[0],
          global::AElf.Contracts.ReceiptMakerContract.ReceiptMakerReflection.Descriptor.Services[0],
          global::AElf.Contracts.Bridge.BridgeContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion
  }
}
#endregion
