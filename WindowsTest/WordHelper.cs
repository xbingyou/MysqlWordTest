using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MSWord = Microsoft.Office.Interop.Word;
using System.IO;
using System.Reflection;
using System.Drawing;

namespace WindowsTest
{
    public class WordHelper
    {
        public static void CreatWordByReport()
        {
            object path;                              //文件路径变量
            string strContent;                        //文本内容变量
            MSWord.Application wordApp;                   //Word应用程序变量
            MSWord.Document wordDoc;                  //Word文档变量
            path = Environment.CurrentDirectory + "\\MyWord_Print1.doc";
            wordApp = new MSWord.ApplicationClass(); //初始化
            wordApp.Visible = false;//使文档可见
            //如果已存在，则删除
            if (File.Exists((string)path))
            {
                File.Delete((string)path);
            }

            //由于使用的是COM库，因此有许多变量需要用Missing.Value代替
            Object Nothing = Missing.Value;
            wordDoc = wordApp.Documents.Add(ref Nothing, ref Nothing, ref Nothing, ref Nothing);

            #region 页面设置、页眉图片和文字设置，最后跳出页眉设置

            //页面设置
            wordDoc.PageSetup.PaperSize = MSWord.WdPaperSize.wdPaperA4;//设置纸张样式为A4纸
            wordDoc.PageSetup.Orientation = MSWord.WdOrientation.wdOrientPortrait;//排列方式为垂直方向
            wordDoc.PageSetup.TopMargin = 57.0f;
            wordDoc.PageSetup.BottomMargin = 57.0f;
            wordDoc.PageSetup.LeftMargin = 57.0f;
            wordDoc.PageSetup.RightMargin = 57.0f;
            wordDoc.PageSetup.HeaderDistance = 30.0f;//页眉位置

            #endregion

            #region 首行标题图片
            //图片文件的路径
            string filenamede = Environment.CurrentDirectory + "\\naba.jpg";
            Object rangede = wordDoc.Paragraphs.Last.Range;
            Object linkToFilede = false; 
            Object saveWithDocumentde = true;
            wordDoc.InlineShapes.AddPicture(filenamede, ref linkToFilede, ref saveWithDocumentde, ref rangede);
            wordDoc.InlineShapes[1].Width = 155;
            wordDoc.InlineShapes[1].Height = 65;
            #endregion

            #region 行间距与缩进、文本字体、字号、加粗、斜体、颜色、下划线、下划线颜色设置
            object unite = MSWord.WdUnits.wdStory;
            wordDoc.Content.InsertAfter("\n");
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            Color color = Color.FromArgb(91, 155, 213);
            MSWord.WdColor colorFont = (MSWord.WdColor)(color.R + 0x100 * color.G + 0x10000 * color.B);
            Color color1 = Color.FromArgb(222, 235, 246);
            MSWord.WdColor colorTable = (MSWord.WdColor)(color1.R + 0x100 * color1.G + 0x10000 * color1.B);

            //写入文本
            strContent = "那百\n";
            //wordDoc.Paragraphs.Last.Range.Font.Name = "微软雅黑";
            wordDoc.Paragraphs.Last.Range.Font.Size = 14;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 1;
            wordDoc.Paragraphs.Last.Range.Font.Color = colorFont;
            wordDoc.Paragraphs.Last.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            wordDoc.Paragraphs.Last.Range.Text = strContent;
            wordDoc.Paragraphs.Last.Range.InsertAfter("免疫检测PCR\n");
            //绘制横线
            MSWord.Paragraph p = wordDoc.Paragraphs.Add(ref Nothing);
            p.Format.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            p.Format.Borders[MSWord.WdBorderType.wdBorderBottom].LineStyle = MSWord.WdLineStyle.wdLineStyleSingle;

            MSWord.Shape hLine;
            hLine = wordDoc.Shapes.AddConnector(Microsoft.Office.Core.MsoConnectorType.msoConnectorStraight, 0, 0, 482f, 0);
            hLine.Line.Weight = 1.5f;
            hLine.Top = 60f;
            hLine.Left = 0;
            hLine.Line.ForeColor.RGB = (int)colorFont;
            hLine = wordDoc.Shapes.AddConnector(Microsoft.Office.Core.MsoConnectorType.msoConnectorStraight, 0, 0, 482f, 0);
            hLine.Line.Weight = 1.5f;
            hLine.Top = 150f;
            hLine.Left = 0;
            hLine.Line.ForeColor.RGB = (int)colorFont;
            hLine = wordDoc.Shapes.AddConnector(Microsoft.Office.Core.MsoConnectorType.msoConnectorStraight, 0, 0, 482f, 0);
            hLine.Line.Weight = 1.5f;
            hLine.Top = 460f;
            hLine.Left = 0;
            hLine.Line.ForeColor.RGB = (int)colorFont;
            hLine = wordDoc.Shapes.AddConnector(Microsoft.Office.Core.MsoConnectorType.msoConnectorStraight, 0, 0, 482f, 0);
            hLine.Line.Weight = 1.5f;
            hLine.Top = 530f;
            hLine.Left = 0;
            hLine.Line.ForeColor.RGB = (int)colorFont;
            #endregion

            //绘制表格
            //wordDoc.Content.InsertAfter("\n");//这一句与下一句的顺序不能颠倒，原因还没搞透
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            wordApp.Selection.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;
            int tableRow = 3;
            int tableColumn = 3;
            wordDoc.Paragraphs.Last.Range.Font.Size = 9;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 0;
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorBlack;
            MSWord.Table table = wordDoc.Tables.Add(wordApp.Selection.Range,
            tableRow, tableColumn, ref Nothing, ref Nothing);
            wordDoc.Tables[1].Cell(1, 1).Range.Text = "宠主姓名：小于";
            wordDoc.Tables[1].Cell(1, 2).Range.Text = "宠宝昵称：小小";
            wordDoc.Tables[1].Cell(1, 3).Range.Text = "宠宝年龄：1年6个月";
            wordDoc.Tables[1].Cell(2, 1).Range.Text = "宠宝类别：猫";
            wordDoc.Tables[1].Cell(2, 2).Range.Text = "宠宝性别：雌";
            wordDoc.Tables[1].Cell(3, 1).Range.Text = "实验编号：K350001";
            wordDoc.Tables[1].Cell(3, 2).Range.Text = "检测时间：2021-07-09 12:31:05";

            wordDoc.Paragraphs.Last.Range.Font.Size = 11;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 1;
            wordDoc.Paragraphs.Last.Range.Font.Color = colorFont;
            wordDoc.Paragraphs.Last.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            wordDoc.Paragraphs.Last.Range.InsertAfter("—— 检测结果 ——");
            wordDoc.Content.InsertAfter("\n");
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾

            //绘制结果表格
            tableRow = 7;
            tableColumn = 4;
            wordDoc.Paragraphs.Last.Range.Font.Size = 9;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 1;
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorWhite;
            MSWord.Table tableRpt = wordDoc.Tables.Add(wordApp.Selection.Range,
            tableRow, tableColumn, ref Nothing, ref Nothing);
            tableRpt.Borders.Enable = 1;
            tableRpt.Borders.InsideColor = MSWord.WdColor.wdColorWhite;
            tableRpt.Borders.OutsideColor = MSWord.WdColor.wdColorWhite;
            
            for (int j = 1; j <= 4; j++)
            {
                tableRpt.Cell(1, j).Range.Shading.BackgroundPatternColor = colorFont;
                tableRpt.Cell(1, j).Height = 16;
            }
            tableRpt.Cell(1, 1).Range.Text = "检测项目";
            tableRpt.Cell(1, 1).Range.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;
            tableRpt.Cell(1, 2).Range.Text = "Ct值";
            tableRpt.Cell(1, 3).Range.Text = "阳性判断值";
            tableRpt.Cell(1, 4).Range.Text = "检测结果";
            for (int i = 2; i <= 7; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    tableRpt.Cell(i, j).Height = 16;
                    if (i >= 5)
                    {
                        tableRpt.Cell(i, j).Range.Font.Size = 9;
                        continue;
                    }
                    if (j == 1)
                        tableRpt.Cell(i, j).Range.ParagraphFormat.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;
                    tableRpt.Cell(i, j).Range.Shading.BackgroundPatternColor = colorTable;
                    tableRpt.Cell(i, j).Range.Text = string.Format("{0}行{1}列", i.ToString(), j.ToString());
                    tableRpt.Cell(i, j).Range.Font.Size = 9;
                    tableRpt.Cell(i, j).Range.Font.Bold = 0;
                    tableRpt.Cell(i, j).Range.Font.Color = MSWord.WdColor.wdColorBlack;
                }
            }
            //绘制曲线图表格
            wordDoc.Content.InsertAfter("\n");
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            wordDoc.Paragraphs.Last.Range.Font.Size = 11;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 0;
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorBlack;
            tableRow = 4;
            tableColumn = 3;
            MSWord.Table tableGraph = wordDoc.Tables.Add(wordApp.Selection.Range,
            tableRow, tableColumn, ref Nothing, ref Nothing);
            //向新添加的行的单元格中添加图片
            Image img;
            for (int i = 1; i <= 3; i++)
            {
                img = Image.FromFile(Environment.CurrentDirectory + "\\naba.jpg");
                System.Windows.Forms.Clipboard.SetDataObject(img, false, 3, 500);
                tableGraph.Cell(1, i).Range.Paste();
                tableGraph.Cell(2, i).Range.Text = "猫杯状病毒";
                tableGraph.Cell(2, i).Height = 25;
            }
            for (int i = 1; i <= 3; i++)
            {
                img = Image.FromFile(Environment.CurrentDirectory + "\\naba.jpg");
                System.Windows.Forms.Clipboard.SetDataObject(img, false, 3, 500);
                tableGraph.Cell(3, i).Range.Paste();
                tableGraph.Cell(4, i).Range.Text = "猫疱疹病毒";
                tableGraph.Cell(4, i).Height = 25;
            }

            wordDoc.Content.InsertAfter("\n");
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾
            wordDoc.Paragraphs.Last.Range.Font.Size = 11;
            wordDoc.Paragraphs.Last.Range.Font.Bold = 0;
            wordDoc.Paragraphs.Last.Range.Font.Color = colorFont;
            wordDoc.Paragraphs.Last.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphCenter;
            wordDoc.Paragraphs.Last.Range.InsertAfter("—— 结果判读 ——\n");
            wordDoc.Paragraphs.Last.Range.Font.Color = MSWord.WdColor.wdColorBlack;
            wordDoc.Paragraphs.Last.Alignment = MSWord.WdParagraphAlignment.wdAlignParagraphLeft;
            wordDoc.Paragraphs.Last.Range.InsertAfter("待测样本的Ct值在参考范围内，且有典型的S形扩增曲线时，检测结果为阳性；\n");
            wordDoc.Paragraphs.Last.Range.InsertAfter("待测样本的Ct值在参考范围外，或者Ct值在参考范围内但无典型的S形扩增曲线时，检测结果为阴性。");
            wordDoc.Content.InsertAfter("\n");
            wordApp.Selection.EndKey(ref unite, ref Nothing); //将光标移动到文档末尾


            //string FileName = Environment.CurrentDirectory + "\\6.png";//图片所在路径
            //object LinkToFile = false;
            //object SaveWithDocument = true;
            //object Anchor = tableGraph.Cell(1, 1).Range;//选中要添加图片的单元格
            //wordDoc.Application.ActiveDocument.InlineShapes.AddPicture(FileName, ref LinkToFile, ref SaveWithDocument, ref Anchor);
            //MSWord.Shape s = wordDoc.Application.ActiveDocument.InlineShapes[2].ConvertToShape();
            //s.WrapFormat.Type = MSWord.WdWrapType.wdWrapSquare;

            //WdSaveFormat为Word 2003文档的保存格式
            object format = MSWord.WdSaveFormat.wdFormatDocument;
            //将wordDoc文档对象的内容保存为DOCX文档
            wordDoc.SaveAs(ref path, ref format, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing, ref Nothing);
            //关闭wordDoc文档对象
            wordDoc.Close(ref Nothing, ref Nothing, ref Nothing);
            //关闭wordApp组件对象
            wordApp.Quit(ref Nothing, ref Nothing, ref Nothing);

            //我还要打开这个文档玩玩
            MSWord.Application app = new MSWord.Application();
            MSWord.Document doc = null;
            try
            {

                object unknow = Type.Missing;
                app.Visible = true;
                //string str = Environment.CurrentDirectory + "\\MyWord_Print.doc";
                object file = path;
                doc = app.Documents.Open(ref file,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow, ref unknow,
                    ref unknow, ref unknow, ref unknow);
                string temp = doc.Paragraphs[1].Range.Text.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
