   M S B A M L   `   `   `  ���� [  WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms  H DTWD Face Demo, Version=6.0.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TwinsDay2017_DemoTwinsDay2017_Demo "   TwinsDay2017_Demo.MainWindow   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     /srcclr-namespace:TwinsDay2017_Demo     -     � Title$8  2WVU Biometrics Face Verfication and Identification��$	��800��$
��1200�����#001C3B.��� -   $��MainGrid��$��20,10,10,10q����#193D66|�.  � FocusVisualStyle Z� .+��� -   $��5q�
�������$��1q�.@��� -    ��Name$ IdentifyTab��$��	0,0,0,2.5q�$��50������ $��Identification��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� � ColumnDefinitions �� $��2* ��� $��3* �.+�]� ��. $��0��.+��� $	��Top=����#6E6259$��2q�._�� 
������� � RowDefinitions �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -   $��IdentifyImport��$	��100��$��28��$��3q�$	��Top=�$��Right=�.���� $��24������  �� DecodePixelHeight$ 24�� �� 	UriSource$( "/Icon Images/Normal/FolderPlus.png?��� -   $��IdentificationImportPath��$	��Top=�$��Center=�$��28��$��3q�]� $��0����. .+��� -   $��IdentifyScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -   $��IdentificationScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+���  �� Orientation$ 
Horizontal=�$��Right=�.+��� -	   $��IdentifySelect��$��Select��$��60��$��25��$��3q�{�.  �� -
   $��IdentifyDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -   $��IdentifyImageSelect��$��3q�{�.  ]� $��Bottom=����#325276.+��� $ 
Horizontal=�$��Center=�$��Bottom=�$��40��.+��� -   $��IdentificationClearWindow��$��Clear��$��63��$��0,0,5,0q�$��16(�{�.  �� -   $��IdentificationReset��$��Reset��$��62��$��5,0,0,0q�$��16(�{�.  �� -   $��GenerateMatches��$��Generate Matches��$��52��$��3q�$��18(�{�.  �� ���#6E6259$��2q�$	��0,3q�._�]� 
�������.+��� -   $��ParticipantLabel��$��16(�$��Waiting for Import��$	��Top=�$	��Top=�$��Center=�$��Italic$�7  1System.Windows.Forms.Integration.WindowsFormsHost  -     Name$ ProbeImagePanel��}�. ��. 	   Child.	 +   %System.Windows.Forms.TableLayoutPanel  ]� ��. $��1��$��20,0,0,0q�.+��� $	��Top=�$	��500��$��0,10,0,0q�$��2q����#6E6259���#D0D4D6._��� $��Top Matches:��$��16(�$��Center=�$	��Top=�$��0,3,3,0q�$��Black"�$��Italic$�� $��Bottom=�$��3q�.+�]� .+��� -   $��IdentStatusInfo��$��Bottom=�$��
Progress: ��$��25��$��Center=�
 �� IsReadOnlyCaretVisible
 .   �� 
IsReadOnly . $��Black"�$��Italic$�
�������� -   $��IdentificationStatus�� � Minimum$ 0Q� � Maximum$	 100Q� � Value$ 0Q�$��35���� -   $��TopMatchList��   resultListStyle#��[  ���#6E6259$��2q�$��3q�
���������. $��Visible=� � IsSharedSizeScope . ���� ���� .R�]� -   $��3q�.+�� $
��Left=� �� $��50���� $
��Auto �.+��� $��0��$	��5,0q�$
��Bold"�$��20(�$��Center=�$��Center=�����  �� Path$
 Rank� �� StringFormat$
 #{0}���� -   $��1�� � Cursor$
 Hand��
 � Tag �� $
 Rank�.���� $	��180������ $ Image�� $
��10,0q� �� $��* ��� $��* ��� $��* ��� $��* �.+��� $��0��$ 
Horizontal=�$��Center=�.+��� $��0q�$��18(�$
��Bold"�$��Italic$�$��RID: ���� $��0q�$��18(����� $ ID��� $��1��$ 
Horizontal=�$��Center=�.+��� $��0q�$��18(�$
��Bold"�$��Italic$�$��Score: ���� $��0q�$��18(����� $ Score��� $��2��$ 
Horizontal=�$��Center=�.+��� $��0q�$��18(�$
��Bold"�$��Italic$�$��False Acceptance Rate: ���� $��0q�$��18(����� $	 FAR�$ {0:0.####}%���� $��3��$ 
Horizontal=�$��Center=�.+��� $��0q�$��18(�$
��Bold"�$��Italic$�$$��False Acceptance Probability: ���� $��0q�$��18(����� $ Probability�$ {0:0.####}%������  �� 
TargetType" ��.���  �� Property � �� �� Value$ Stretch=� �� Triggers P�  P� Property �@��
IsSelected P� Value . . ���  � ��
 �   ��� -   $ 	VerifyTab��$��0,2.5q�$��50������ $��
Verify 1:1��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� �� $��3* ��� $��2* ��� $��3* �.+�]� $��0����. $��0,0,5,0q�.+��� $	��Top=����#6E6259$��2q�._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -    ��Name$ 
LeftImport��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -    ��Name$ LeftImportPath��$��3q�$��Center=�]� $��0����. .+��� -   $ LeftScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -   $ LeftScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -   $ 
LeftSelect��$��Select��$��60��$��25��$��3q�{�.  �� -   $ LeftDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -    ��Name$ LeftImageSelect��$��3q�{�.  �� ���#6E6259$��2q�$	��0,3q�._�  -   $ LeftImagePanel��   
Background
 �����.	   ]� $��1��$	��5,0q���. .+��� $	��Top=����#325276$��0,0,0,10q�.+��� -   $ VerifyImages��$��Verify��$��5q�{�.  $��65��$��18(��� $��Bottom=�$��	0,2.5,0,0q����#325276.+��� -    $ VerificationStore��$y�	Collapsed=�$��Store Results��{�.  $��55��$	��2.5q�$��18(��� $ 
Horizontal=�$��Center=�$��45��.+��� -!   $ VerificationClearLeft��$	��2.5q�$��50��{�.  .����  �� TextAlignment$ Center=��� $�Clear���� �� $
�Left���� -"   $ VerificationClearRight��$	��2.5q�$��50��{�.  .���� $ Center=��� $�Clear���� �� $�Right���� -#   $ VerificationClear��{�.  $	��2.5q�$��50��.���� $ Center=��� $�Clear���� �� $
�Both���� -$   $ VerificationReset��$��Reset��{�.  $	��2.5q�$��50���� ���#6E6259$��2q�$	��0,3q�._�]� ��. .+��� $��Results:��$	��Top=����#D0D4D6$��16(�$��Black"�$��Italic$��� -%   $ VerifyResults��$��16(�
�������
 .  . ]� $��2����. $��5,0,0,0q�.+��� $	��Top=����#6E6259$��2q�._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -&   $ RightImport��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -'   $ RightImportPath��$��3q�$��Center=�]� $��0����. .+��� -(   $ 	RightScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -)   $ RightScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -*   $ RightSelect��$��Select��$��60��$��25��$��3q�{�.  �� -+   $ RightDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -,   $ RightImageSelect��$��3q�{�.  �� ���#6E6259$��2q�$	��0,3q�._�  --   $ RightImagePanel��
 �����.	   �� -.   $ 
VerifyNTab��$��0,2.5q�$��50������ $��
Verify 1:N��.���� $��-5q�$��3q����#2C2A29���#002855._�� $��4q� �� $��* ��� $��* �.+�]� $��0��.+�]� $��Bottom=����#325276.+��� $ 
Horizontal=�$��40��$��Center=�$��Bottom=�.+��� -/   $ VerifySubject_Clear��$��Clear��$��80��$��0,0,5,0q�$��16(�{�.  �� -0   $ VerifySubject_Reset��$��Reset��$��80��$��5,0,0,0q�$��16(�{�.  �� -1   $ VerifySubjectImage��$	��Top=�$��Verify��$��52��$��3q�$��18(�{�.  ]� $	��Top=�.+��� $	��Top=����#6E6259$��2q�._�� 
������� �� $��* ��� $��* ��� $��* �.+�]� $��2����. .+��� -2   $ VerifyImport��$��Right=�$��30��$	��100��$��3q�.���� $��24������ $ 24��$( "/Icon Images/Normal/FolderPlus.png?��� -3   $ VerifyImportPath��$��3q�$��Center=�]� $��0����. .+��� -4   $ 
VerifyScan��$
��Scan��$	��100��$��28��$��3q�$	��Top=�$��Right=��� -5   $ VerifyScanPath��$	��Top=�$��Center=�$��28��$��3q�]� $��1��.+��� $ 
Horizontal=�$��Right=�.+��� -6   $ VerifySelect��$��Select��$��60��$��25��$��3q�{�.  �� -7   $ VerifyDeselect��$��Deselect��$��60��$��25��$��3q�{�.  �� -8   $ VerifyImageSelect��$��3q�{�.  �� ���#6E6259$��2q�$	��0,3q�._�  -9   $ VerifyImagePanel��
 �����.	   ]� $��1��$��15,0,0,0q�.+��� -:   $ VerifyStoreResult��$y�	Collapsed=�$��Bottom=�$��Store Results��$��55��$��10q�$��16(�{�.  �� $	��Top=�$��3,10,3,0q����#D0D4D6$��2q����#6E6259._��� $��Results:��$	��0,3q�$��Center=�$��16(�$
��Bold"�$��Italic$��� -;   $ VerifyN_Results��$��16(�
�������$��3q����#6E6259$��2q���. $��Visible=� �� TextWrapping$
 Wrap=�
 .  . �� -<   $ TemplateTab��$��0,2.5q�$��50������ $��Generate Templates��.���� $��-5q�$��3q����#2C2A29���#002855._�]� $��4q�.+�]� $��Bottom=�.+�� $��Right=����#325276 �� $��50 ��� $��35 �.+��� $��0��$ 
Horizontal=�$��Right=�.+��� -=   $��GenerateAll��$��Generate All Templates��$	��138��$��40��$��5,5,2.5,2.5q��� ->   $��GenerateSelected��$!��Generate Selected Templates��$	��173��$��40��$��2.5,5,5,2.5q��� -?   $��CancelTemplate��$��1��$��Cancel��{�.  $��	5,2.5,5,5q�� $
��Left=�$
��20,5q�.+�]� .+��� -@   $��TemplateStatusInfo��$��Bottom=�$��
Progress: ��$��25��$��Center=�
 .   . $��Black"�$��Italic$�
�������� -A   $��TemplateStatus��$ 0Q�$	 100Q�$ 0Q�$��40��]� $	��Top=����#A2AAAD.+��� $��Bottom=�$��4q�$
��Bold"�$��Italic$�$(��"Select directories to be searched:���� 
�������$��1q����#B0B0B0$��
10,10,10,0q�  J� Effect  3  -System.Windows.Media.Effects.DropShadowEffect  !   
BlurRadius$! 2Q�"   ShadowDepth$" 2Q�#   Color$# DarkGray��._��� $��5q�.+��� $��14(�$��SemiBold"�$��Italic$�$��5q�Please Note:�� $��14(�$
��Wrap=�$��5q���Template generation will traverse through selected directories to record subject information and generate a biometric template for that given subject. When evaluating selected directories, only subjects and data within those directories will be re-evaluated. Template data is used for identification, so any subjects that have not yet been evaluated will be omitted during the enrollment phase of the identification process.�� -B   $��DatabaseDisplay��$ �� SelectionMode$$ Multiple=����#6E6259$��2q�
�������$	��0,4q���. $��Visible=��� -C   $ SettingsTab��$��0,2.5q�$��50������ $��Settings��.���� $��-5q�$��3q����#2C2A29���#002855._�]� $��4q�.+��� $ 
Horizontal=�$��Right=�$��Bottom=�$��Bottom=�$��36�����#325276.+��� -D   $ ApplySetting��$��Apply��$��90��$��2.5,0q�{�.  �� -E   $ CancelSetting��$��Cancel��$��90��$��2.5,0q�{�.  �� -F   $ RevertSetting��$��Revert��$��90��$��2.5,0q�{�.  �� -G   $ DefaultSetting��$��Default��$��90��$��2.5,0q�{�.  ]� $	��Top=����#A2AAAD.+��� $	��Top=�$��8q�
�������$��1q����#B0B0B0    $! 2Q�$" 2Q�$# DarkGray��._��� .+��� $��14(�$��SemiBold"�$��Italic$�$��5q��� $�Please Note:���� $��14(�$
��Wrap=�$��5q��� $���When changing any settings or adding/removing directories, make sure to apply them or your changes will not be retained. If, for some reason, you accidentally apply an unwanted change, you can select the Revert button to return back to the previously applied settings.��]� $��Bottom=�$��	0,7.5,0,5q�.+��� $ 
Horizontal=�$��Right=�.+��� $��Set TWD 2017 Server:��$��Center=�$��Black"�$��Italic$�$
 Help��% ^� InitialShowDelay$
% 1000��& ^� ShowDuration$& 10000��' ^� BetweenShowDelay$
' 2000��( ^� 	IsEnabled( . ) � ToolTip) _� $	��350��.���� .+��� $
��Bold"��� $�Twins Day 2017 Server���� $
��Wrap=��� $���Creates a dialog box to set the 2017 Twins Day server. When the dialog box opens, it will already display the currently set server addesss. Please note that the address entered should correspond to the root address of the server, not the address to a modality folder.���� �� �� $���The server set will correspond to image/voice aquisition for both template processing and for demo purposes, such as obtaining probe faces/voices for a specific RID.���� -H   $ 
SetTWD2017��$��45��$
��10,0q�.���� $��24������ $ 24��$" /Icon Images/Normal/Gear.png?��� $ 
Horizontal=�$
��Left=�.+��� $#��Directories to Match Against:��$��Center=�$��Black"�$��Italic$�$
 Help��$
% 1000��$& 10000��$
' 2000��( . ) _� $	��350��.���� .+��� $
��Bold"��� $�Directories/Networks���� $
��Wrap=��� $���The list of directories used to search for and record subjects when generating templates. If there is a directory or network path that is not present, you can search through folder structures (Add Directory) or add a network by addesss (Add Network/Server).���� �� �� $���Please note that directory and network paths should point to the folders which contain all of the RID folders. If this is not the case, it may cause errors during the template process.���� -I   $ AddDirectory��$��Add Directory��$��32��$	��105��$��2.5,0q��� -J   $ AddNetworkDrive��$��Add Network/Server��$	��148��$��2.5,0q��� -K   $ RemoveDatabase��$��Remove Selected��$��32��$	��130��$��2.5,0q��� -L   $ ClearDatabase��$��	Clear All��$��32��$��70��$��2.5,0q��� $ 
Horizontal=�$��	0,5,0,7.5q�.+��� $'��!Number of Top Matches to Display:��$��2q�$��Black"�$��Italic$�$
 Help��$
% 1000��$& 10000��$
' 2000��( . ) _� $	��350��.���� .+��� $
��Bold"��� $�Top Matches���� $
��Wrap=��� $���The top matches will set the maximum possible results returned during the identification process. However, if there are fewer matches than the maximum, only those top matches will be displayed.���� -M   $ NumOfMatches��* �� 
IsEditable* . + �� IsTextSearchEnabled+ .  $��60��$��2q��� $,��&Matching False Acceptance Rate ( % ): ��$��5,2,2,2q�$��Black"�$��Italic$�$
 Help��$
% 1000��$& 10000��$
' 2000��( . ) _� $	��350��.���� .+��� $
��Bold"��� $!�False Acceptance Rate (FAR)���� $
��Wrap=��� $���FAR is the percent likelihood that an incorrect match will be accepted as a match. So if FAR is set larger, then it will be more likely that a false match will be made.���� �� �� $���The FAR is related to the matching threshold. So the higher the FAR is, the lower the matching score/threshold will be, and vice versa. To finalize any changes, either press enter or click outside the control.���� -N   $ setMatchFAR��* . + .  $	��110��$��2q��� $$��Matching Threshold ( Score ): ��$��5,2,2,2q�$��Black"�$��Italic$�$
 Help��$
% 1000��$& 10000��$
' 2000��( . ) _� $	��350��.���� .+��� $
��Bold"��� $�	Threshold���� $
��Wrap=��� $���The matching score/threshold is the minimum score that both identification and verification functions use to assume that a subject is a match. Any comparisons that return a comparison score that is below the threshold, the resulting score will be set to zero.���� �� �� $���The threshold is related to the FAR. So the higher the score, the lower the FAR, and vice versa. To finalize any changes, either press enter or click outside the control.���� -O   $ setThreshold��$��75��$��2q�� $	��0,4q� �� $��40 ��� $��* �.+��� -P   , ��Name$, DBStatusList��$��0����. $��Disabled=�$��Disabled=�$��2,2,1,2q����#6E6259
����������� *Items+- �� ElementName$- DatabaseDisplayList������ .R��� -Q   $��25��$��25��$	��2.5q�
����·�$��Center=�$��Center=� �� . �� Mode$. OneWay=�.���� $��16������ / �� DecodePixelWidth$/ 16��$$ /Icon Images/Small/Refresh.png?����� " ��.���  � |� .  ��  � ��$ Center=��� -R   $, DatabaseDisplayList��$��1��$$ Multiple=�$��0,2,2,2q����#6E6259
���������. $��Visible=����� " ��.���  � ��$ 30���� -S   $ LogTab��$��0,2.5q�$��50������ $
��Logs��.���� $��-5q�$��3q����#2C2A29���#002855._��� -T   $ LogText��$��10q���. $��Visible=�$
 Wrap=�
 .  . 
����������#6E6259$��2q�