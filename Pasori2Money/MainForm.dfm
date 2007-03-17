object MForm: TMForm
  Left = 233
  Top = 123
  BorderStyle = bsSingle
  Caption = 'Pasori2Money'
  ClientHeight = 79
  ClientWidth = 417
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
    Left = 24
    Top = 24
    Width = 89
    Height = 25
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
    Left = 256
    Top = 24
    Width = 49
    Height = 25
    Caption = #32066#20102
    TabOrder = 1
    OnClick = ButtonQuitClick
  end
  object ButtonConfig: TButton
    Left = 144
    Top = 24
    Width = 75
    Height = 25
    Caption = #35373#23450
    TabOrder = 2
    OnClick = ButtonConfigClick
  end
  object OpenDialog: TOpenDialog
    Filter = 'SFCPeep |SFCPeep.exe'
    Left = 336
    Top = 16
  end
end
