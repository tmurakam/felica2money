object MForm: TMForm
  Left = 233
  Top = 123
  BorderStyle = bsSingle
  Caption = 'Pasori2Money'
  ClientHeight = 57
  ClientWidth = 372
  Color = clBtnFace
  Font.Charset = SHIFTJIS_CHARSET
  Font.Color = clWindowText
  Font.Height = -12
  Font.Name = #65325#65331' '#65328#12468#12471#12483#12463
  Font.Style = []
  OldCreateOrder = False
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 12
  object ButtonConvert: TButton
    Left = 8
    Top = 8
    Width = 89
    Height = 33
    Caption = #21462#12426#36796#12415
    Font.Charset = SHIFTJIS_CHARSET
    Font.Color = clWindowText
    Font.Height = -12
    Font.Name = #65325#65331' '#65328#12468#12471#12483#12463
    Font.Style = [fsBold]
    ParentFont = False
    TabOrder = 0
    OnClick = ButtonConvertClick
  end
  object ButtonQuit: TButton
    Left = 207
    Top = 8
    Width = 49
    Height = 33
    Caption = #32066#20102
    TabOrder = 1
    OnClick = ButtonQuitClick
  end
  object ButtonConfig: TButton
    Left = 112
    Top = 8
    Width = 75
    Height = 33
    Caption = #35373#23450
    TabOrder = 2
    OnClick = ButtonConfigClick
  end
  object ButtonHelp: TButton
    Left = 286
    Top = 8
    Width = 75
    Height = 33
    Caption = #12504#12523#12503
    TabOrder = 3
    OnClick = ButtonHelpClick
  end
  object OpenDialog: TOpenDialog
    Filter = 'SFCPeep '#23455#34892#12501#12449#12452#12523' (SFCPeep.exe)|SFCPeep.exe'
    Left = 176
    Top = 32
  end
end
