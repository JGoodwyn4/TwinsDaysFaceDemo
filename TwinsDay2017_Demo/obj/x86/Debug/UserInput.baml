   M S B A M L   `   `   `  ����  TWD Face Demo!    TwinsDay2017_Demo.UserInput   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Input Prompt��5   	    � SizeToContent$ WidthAndHeight=�6   $	��500��6=   $	��350��6L    � 
ResizeMode$ NoResize=�5   U    � ShowInTaskbar .  � WindowStartupLocation$ CenterOwner=�5      .��5      � ���#EAECEF � RowDefinitions 5	   
   �� 5
      $��* �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   .+�5   
   � $��0��6   $��10q�6    � ColumnDefinitions 5      �� 5      $��* �6#   �� 5      $��3* �6#   .+�5      �� -   5        $��	InputIcon��5   $   $��0��6   $
��Left=�65   $��40��6K   $��Center=�6Y   �� 5      -   5        $��InputQuestion��5   (   $��1��6   $��15(�6=   $��SemiBold"�6K   $��Italic$�6a   $
��10,0q�6t   $��WrapWithOverflow=�6�   $��Center=�6�   5      � 5   
   $��1��6   $��10q�6    5      �� 5      $
��Auto �6#   �� 5      $��* �6#   .+�5      �� -   5        $��
TextPrompt��5   $   $��0��6   $
��Left=�66   $��Input: ��6L   $��13(�6^   $��Medium"�6l   �� 5!      -   5         ��Name$ InputWrapper��5!   %   $��1��6   $��5,0,10,0q�6;   �� 5"      -   5        $��UserTextInput��5"   &   $��1��6   $��30��6;   $��5,0,10,0q�6   $��Center=�6�    �� TextWrapping$ NoWrap=�6�   ��6G   �� *ActualWidth+	 �� ElementName$	 InputWrapper��6�   5#      �� 5%   
   $��2��6   
 �� Orientation$
 
Horizontal=�6"   $��Center=�6;   $��
20,5,20,10q�6X   .+�5&      �� -   5        $��InputOK��5&      $��Ok��6$    �� 	IsDefault . $��50��6B   $��30��6M   $	��5,0q�6Y   6f   �� 5'      -   5        $��InputCancel��5'      $��Cancel��6(    �� IsCancel . $��80��6I   $	��5,0q�6T   6a   5(      5)      5+      