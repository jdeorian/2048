   M S B A M L   `   `   `  ����  _2048_c_sharp.GUI"    _2048_c_sharp.GUI.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   X TXceed.Wpf.Toolkit, Version=3.4.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)    � 	Resources 5
         DataTemplate_Level2  DataTemplate_Level1 �� Orientation��%,  &	        &	 �     �� 5   
   .R�5      �� $��40��6(   $��50��64   
���   �$��1q�6S   $��4,4,4,4q�6g   ��6   �� 6g   5      �� 5   
   .R�5      �� #���   63   ��6   �� ��5      �� 5      .R�5      �� $ 
Horizontal=�6%   5      5      5      5      .��5      � .+�5   
   %  Xceed.Wpf.Toolkit.IntegerUpDown  -   5        $��iudConcurrentBoardCount��5      $
��Left=�6<   $��
130,13,0,0q�6W   $	��Top=�6k   $��75��6�   �  �Xceed.Wpf.Toolkit.NumericUpDown`1[[System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   	MaxLength$ 2��6�   �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Minimum$ 1X�6�      Maximum$ 48X�6�      Value$ 4X�6�   6�   �� 5   
   $��Concurrent threads:��6   $
��Left=�6.   $��	10,10,0,0q�6I   $	��Top=�6\   �� 5   
   -   5        $��Start��5      $
��Left=�6!   $��
707,10,0,0q�6<   $	��Top=�6P   $��75��6h   4  .System.Windows.Automation.AutomationProperties   Name$ btnStart��6�   �� 5   
   $��Threads:��6   $
��Left=�6#   $��	10,52,0,0q�6>   $	��Top=�6Q   $
��Bold"�6i   &   System.Windows.Controls.DataGrid  5   
   $
��Left=�6   $	��326��6.   $��	10,83,0,0q�6;   $	��Top=�6N   $	��487��6f   $ 	dgThreads��6r   �� 5    
   $��Best Board:��6   $
��Left=�6&   $��
509,52,0,0q�6A   $	��Top=�6U   $
��Bold"�6m   �� 5!   
   $��Score:��6   $
��Left=�6!   $��
502,83,0,0q�6<   $	��Top=�6P   �� 5"   
   $��0��6   $
��Left=�6   $��
549,83,0,0q�67   $	��Top=�6K   $ 
lblBBScore��6c   �� 5#   
   $��Moves:��6   $
��Left=�6!   $��502,114,0,0q�6<   $	��Top=�6Q   �� 5$   
   $��0��6   $
��Left=�6   $��549,114,0,0q�67   $	��Top=�6L   $ 
lblBBMoves��6d   � 5%   
   $
��Left=�6   $	��264��6*   $��509,145,0,0q�67   $	��Top=�6L   $	��273��6d    � ColumnDefinitions 5&      �� 5'      $��0* �6#   �� 5(      .+�5*      �� -   5        	 ��Name$		 lst��5*      #���  6(   $��2��6]   5+      5-      5.      