   M S B A M L   `   `   `  ����  ScreenRecorder    ScreenRecorder.EmailForm   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F BNHotkey.Wpf, Version=1.2.1.0, Culture=neutral, PublicKeyToken=nullF 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  
Email Form��5   	   $	��500��6   
���   �$	��4pxq�6=   $	��400��6S   $	��590��6_    � 
ResizeMode$ NoResize=�6n    � WindowStyle$
 None=�6�    � WindowStartupLocation$ CenterScreen=�6�    � 	Resources 5       �� 
TargetType �� Property �� Value��%�   '��       �� 5   
   " ��6   .�5	      ��  � ��$ 40��6)   �� 5
       � ��$ 40��6*   �� 5       � ��$ 3q�6'   5      5      .��5      �  � ColumnDefinitions 5   
   �� 5      $��* �6   6+   .+�5   
   �� $��10q�6   .+�5      �� -   5        	 ��Name$	 
LabelTitle��5      $��15(�6(   $
��Bold"�66   .��6H   Enter your Gmail informations6g   �� 5      -   5        $	 LblEmailSucceed��5      
��� � �.��6@   Your E-mail has been sent !6]   �� 5      -   5        $	 LabelYourEmail��5      .��6-   Your E-mail:6;   �� 5      -   5        
 ��Name$
 TxtYourEmail��5       � ToolTip$ Your E-mail��6,   �� 5      -   5        $	 LabelYourPassword��5      .��6/   Your Password:6?   G� 5      -   5         G�Name$ PasswordBoxYourPassword��5      $ Your Password��6;   6U   �� 5      -   5        $	 LabelDestinationEmail��5      .��63   Destination E-mail:6H   �� 5      -   5        $
 TxtEmailDestination��5      $ Email Destination��63   6Q   �� 5      -	   5        $	 LabelSubjectEmail��5      .��6/   Suject E-mail:6?   �� 5      -
   5        $
 TxtSubjectEmail��5      $ Suject E-mail��6/   �� 5      -   5        $	 LabelBodyEmail��5      .��6,   Body E-mail:6:   �� 5!      -   5        $
 TxtBodyEmail��5!       �� TextWrapping$
 Wrap=�6,    �� AcceptsReturn . $ Body E-mail��6U   $	��109��6k   �� 5"       �� Orientation$ 
Horizontal=�6   $��Right=�62   $��49��6N   $	��184��6Z   .+�5#      �� -   5         ��Name$ BtnSendEmail��5#      $
��Send��6/   $��85��6>   $��Center=�6I   $��42��6�   �� 5$      -   5        $ BtnQuitSendEmail��5$      $
��Back��63   $��85��6B   $��Center=�6M   $��42��6�   5%      �� 5'      -   5        $	 LblErrorYourEmail��5'      $
��Bold"�6/   
���  ��6T   �� 5(      -   5        $	 LblErrorYourPassword��5(      $
��Bold"�62   
���  ��6W   �� 5)      -   5        $	 LblErrorEmailDestination��5)      $
��Bold"�66   
���  ��6[   �� 5*      -   5        $	 LblErrorEmailSubject��5*      $
��Bold"�62   
���  ��6W   �� 5+      -   5        $	 LblErrorEmailBody��5+      $
��Bold"�6/   
���  ��6T   5,      5-      5.      