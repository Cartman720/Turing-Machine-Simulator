   M S B A M L   `   `   `  ���� H  DTuringMachine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:TuringMachineTuringMachine      TuringMachine.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -localclr-namespace:TuringMachine     -     � Title$  
MainWindow��$	��700��$
��1000�� � WindowStartupLocation$ CenterScreen=� � AllowsTransparency .  � WindowStyle$
 None=����#2C82C9.���  � RowDefinitions �� $��35 ��� �� $
��0.2* � � ColumnDefinitions �� $
��0.7* ��� $
��0.3* �.+�]� $��2����.  ���#344146.+��� -   $	����   WindowButton#���   $��Right=����#344146
��������� -   $	����#���   $��Right=����#344146
��������� -   $	����#���   $��Right=����#344146
��������� -    ��Name$ Editor��$��1��$��25q�$��15q����#3E4651$��0q�
������� J� Effect 3  -System.Windows.Media.Effects.DropShadowEffect     ShadowDepth$ 0Q�	   
BlurRadius$	 10Q�
   Color$
 #E22211��]� $��2��$��25,0,0,0q���.  $
��Left=�$��Center=�.+��� -    ��Name$ 	RunButton��$	��Run���� -    ��Name$ Strip��$��1��$��1��$��1010101��$
��0,50q�$��35(�$
��Bold"�
������ $��0q�$	��Top=�$��Stretch=�$��Center=�� -    �Name$ Cursor��$��1��$��1��$
��0,95q� � Points �    -1 -1���  -1
���   �
���   �   $ 0Q�$	 10Q�$
 #E22211���� -	    ��Name$ 
ErrorsList��$��1��$��1��$��Center=����� .R�]� ��.  .+��� ���� *Line+�� ���� *ErrorMessage+