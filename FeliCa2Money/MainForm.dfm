object MForm: TMForm
  Left = 233
  Top = 123
  BorderIcons = [biSystemMenu, biMinimize]
  BorderStyle = bsSingle
  Caption = 'FeliCa2Money'
  ClientHeight = 246
  ClientWidth = 396
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
  object Label1: TLabel
    Left = 127
    Top = 21
    Width = 241
    Height = 36
    Caption = 
      'Pasori('#12497#12477#12522')'#12434#20351#12387#12390#38651#23376#12510#12493#12540#12398#21033#29992#23653#27508#12434' Microsoft Money '#12395#21462#12426#36796#12415#12414#12377#12290#12459#12540#12489#12434'Pasori'#12395#32622#12356 +
      #12390#12363#12425#12508#12479#12531#12434#25276#12375#12390#12367#12384#12373#12356#12290
    WordWrap = True
  end
  object Label2: TLabel
    Left = 127
    Top = 97
    Width = 241
    Height = 12
    Caption = #22806#37096#12484#12540#12523'(SFCPeep) '#12398#21033#29992#35373#23450#12434#34892#12356#12414#12377#12290
  end
  object Label3: TLabel
    Left = 127
    Top = 157
    Width = 205
    Height = 12
    Caption = 'Pasori2Money '#12398#12510#12491#12517#12450#12523#12434#38283#12365#12414#12377#12290
  end
  object ButtonConvert: TButton
    Left = 24
    Top = 21
    Width = 89
    Height = 40
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
    Left = 149
    Top = 208
    Width = 84
    Height = 30
    Caption = #32066#20102
    TabOrder = 1
    OnClick = ButtonQuitClick
  end
  object ButtonConfig: TButton
    Left = 24
    Top = 84
    Width = 89
    Height = 41
    Caption = #35373#23450
    TabOrder = 2
    OnClick = ButtonConfigClick
  end
  object ButtonHelp: TButton
    Left = 24
    Top = 144
    Width = 89
    Height = 41
    Caption = #12510#12491#12517#12450#12523
    TabOrder = 3
    OnClick = ButtonHelpClick
  end
  object OpenDialog: TOpenDialog
    Filter = 'SFCPeep '#23455#34892#12501#12449#12452#12523' (SFCPeep.exe)|SFCPeep.exe'
    Left = 80
    Top = 200
  end
end
