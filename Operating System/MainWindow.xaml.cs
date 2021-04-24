using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Operating_System
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    Thread thread1;
    Thread thread2;
    Thread thread3;
    Thread readerThread;
    bool firstFileIsUsed = false;
    bool secondFileIsUsed = false;
    bool thirdFileIsUsed = false;
    bool done, done1, done2, done3, done4;
    ChromeOptions chromeOptions;
    ChromeDriver chromeDriver;
    JsonTemplate1 LastWrittenPost1;
    JsonTemplate2 LastWrittenPost2;
    JsonTemplate3 LastWrittenPost3;

    public MainWindow()
    {
      InitializeComponent();
    }
    private void StartWork()
    {
      done = false;
      done1 = false;
      done2 = false;
      done3 = false;
      done4 = false;
      LastWrittenPost1 = new JsonTemplate1();
      LastWrittenPost2 = new JsonTemplate2();
      LastWrittenPost3 = new JsonTemplate3();
      List<IWebElement> webElements = chromeDriver.FindElements(By.CssSelector("div[class='feed_row '")).ToList();
      List<IWebElement> posts1 = (from t in webElements where (t.FindElements(By.CssSelector("div[class='wall_post_text']")).Count != 0 || (t.FindElements(By.CssSelector("div[class='wall_post_text zoom_text']")).Count != 0)) && (t.FindElements(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).Count != 0) select t).ToList();
      List<IWebElement> posts2 = (from t in webElements where (t.FindElements(By.CssSelector("a[aria-label='фотография']")).Count != 0) && (t.FindElements(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).Count != 0) select t).ToList();
      List<IWebElement> posts3 = (from t in webElements where (t.FindElements(By.CssSelector("a[aria-label='фотография']")).Count != 0) && (t.FindElements(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).Count != 0) select t).ToList();

      thread1 = new Thread(new ParameterizedThreadStart(WriteIdAndText));
      thread1.Name = "Writer1";
      thread2 = new Thread(new ParameterizedThreadStart(WriteIdPostAndPicture));
      thread2.Name = "Writer2";
      thread3 = new Thread(new ParameterizedThreadStart(WriteIdAndHrefPicture));
      thread3.Name = "Writer3";
      readerThread = new Thread(Read);
      readerThread.Name = "Reader";
      thread1.Start(posts1);
      thread2.Start(posts2);
      thread3.Start(posts3);
      readerThread.Start();
    }

    private void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
      if(!done)
      {
        return;
      }
      chromeDriver.Navigate().Refresh();
      Thread mainThread = new Thread(StartWork);
      mainThread.Start(); 
      //throw new NotImplementedException();
    }

    private string CutString(string s)
    {
      s = s.Substring(s.IndexOf("url(") + 4, s.Length - s.IndexOf("url(") - 6);
      return s;
    }
    private void WriteIdAndText(object webElements)
    {
      if(firstFileIsUsed)
      {
        return;
      }
      firstFileIsUsed = true;
      using (StreamWriter streamWriter = new StreamWriter(@"C:\Users\Warmi\Desktop\ОС\text1.txt", true))
      {
        JsonTemplate1 jsonTemplate = new JsonTemplate1();

        foreach (IWebElement item in (List<IWebElement>)webElements)
        {
          jsonTemplate.id = item.FindElement(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).GetAttribute("id").ToString();
          if (item.FindElements(By.ClassName("wall_post_text")).Count != 0)
          {
            jsonTemplate.text = item.FindElement(By.ClassName("wall_post_text")).Text;
          }
          if (item.FindElements(By.CssSelector("div[class='wall_post_text zoom_text']")).Count != 0)
          {
            jsonTemplate.text = item.FindElement(By.CssSelector("div[class='wall_post_text zoom_text']")).Text;
          }
          streamWriter.Write(JsonConvert.SerializeObject(jsonTemplate));
        }
        streamWriter.Flush();
        streamWriter.Dispose();
        streamWriter.Close();
      }
      firstFileIsUsed = false;
      done1 = true;
      done = done1 && done2 && done3 && done4;
      readerThread.Resume();
    }

    private void WriteIdPostAndPicture(object webElements)
    {
      if(secondFileIsUsed)
      {
        return;
      }
      secondFileIsUsed = true;
      using (StreamWriter streamWriter = new StreamWriter(@"C:\Users\Warmi\Desktop\ОС\text2.txt", true))
      {
        JsonTemplate2 jsonTemplate = new JsonTemplate2();
        foreach (IWebElement item in (List<IWebElement>)webElements)
        {
          jsonTemplate.id = item.FindElement(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).GetAttribute("id").ToString();

          foreach(IWebElement picture in item.FindElement(By.CssSelector("div[class='page_post_sized_thumbs  clear_fix']")).FindElements(By.CssSelector("a[aria-label='фотография']")).ToList())
          {
            jsonTemplate.picrureId.Add(picture.GetAttribute("data-photo-id").ToString());
          }
          streamWriter.Write(JsonConvert.SerializeObject(jsonTemplate));
          jsonTemplate.picrureId.Clear();
        }
        streamWriter.Flush();
        streamWriter.Dispose();
        streamWriter.Close();
      }
      secondFileIsUsed = false;
      done2 = true;
      done = done1 && done2 && done3 && done4;
      readerThread.Resume();
    }

    private void WriteIdAndHrefPicture(object webElements)
    {
      if(thirdFileIsUsed)
      {
        return;
      }
      thirdFileIsUsed = true;
      using(StreamWriter streamWriter = new StreamWriter(@"C:\Users\Warmi\Desktop\ОС\text3.txt", true))
      {
        JsonTemplate3 jsonTemplate = new JsonTemplate3();
        foreach (IWebElement item in (List<IWebElement>)webElements)
        {
          jsonTemplate.id = item.FindElement(By.CssSelector("div[class='_post post page_block post--with-likes deep_active']")).GetAttribute("id").ToString();

          foreach (IWebElement picture in item.FindElement(By.CssSelector("div[class='page_post_sized_thumbs  clear_fix']")).FindElements(By.CssSelector("a[aria-label='фотография']")).ToList())
          {
            jsonTemplate.hrefPicture.Add(picture.GetAttribute("data-photo-id").ToString());
          }

          jsonTemplate.hrefPicture.Add(CutString(item.FindElement(By.CssSelector("a[aria-label='фотография']")).GetAttribute("style").ToString()));
          
          streamWriter.Write(JsonConvert.SerializeObject(jsonTemplate));
          jsonTemplate.hrefPicture.Clear();
        }
        streamWriter.Flush();
        streamWriter.Dispose();
        streamWriter.Close();
      }
      thirdFileIsUsed = false;
      done3 = true;
      done = done1 && done2 && done3 && done4;
      readerThread.Resume();
    }

    private void Read()
    {
      if(thread1.IsAlive)
      {
        Thread.CurrentThread.Suspend();
      }

      firstFileIsUsed = true;
      using (StreamReader streamReader = new StreamReader(@"C:\Users\Warmi\Desktop\ОС\text1.txt"))
      {
        string s = @"";
        s += streamReader.Read(); 
        while (s[s.Length - 1] != '}')
        {
          s += streamReader.Read();
        }
        LastWrittenPost1 = JsonConvert.DeserializeObject<JsonTemplate1>(s);
        streamReader.Dispose();
        streamReader.Close();
      }
      firstFileIsUsed = false;

      if (thread2.IsAlive)
      {
        Thread.CurrentThread.Suspend();
      }
      secondFileIsUsed = true;
      using (StreamReader streamReader = new StreamReader(@"C:\Users\Warmi\Desktop\ОС\text2.txt"))
      {
        string s = @"";
        s += streamReader.Read();
        while (s[s.Length - 1] != '}')
        {
          s += streamReader.Read();
        }
        LastWrittenPost2 = JsonConvert.DeserializeObject<JsonTemplate2>(s);

        streamReader.Dispose();
        streamReader.Close();
      }
      secondFileIsUsed = false;

      if (thread3.IsAlive)
      {
        Thread.CurrentThread.Suspend();
      }
      thirdFileIsUsed = true;
      using (StreamReader streamReader = new StreamReader(@"C:\Users\Warmi\Desktop\ОС\text3.txt"))
      {
        string s = @"";
        s += streamReader.Read();
        while (s[s.Length - 1] != '}')
        {
          s += streamReader.Read();
        }
        LastWrittenPost3 = JsonConvert.DeserializeObject<JsonTemplate3>(s);

        streamReader.Dispose();
        streamReader.Close();
      }
      thirdFileIsUsed = false;
      done4 = true;
      done = done1 && done2 && done3 && done4;
    }
    private void Button_Click(object sender, RoutedEventArgs e)
    {
      chromeOptions = new ChromeOptions();
      chromeOptions.AddArgument(@"user-data-dir=C:\Users\Warmi\AppData\Local\Google\Chrome\User Data");
      chromeDriver = new ChromeDriver(chromeOptions);
      chromeDriver.Navigate().GoToUrl(@"https://vk.com/feed");

      done = true;
      System.Timers.Timer timer = new System.Timers.Timer();
      timer.Interval = 2000;
      timer.Enabled = true;
      timer.Elapsed += Timer_Elapsed;
      timer.Start();
      //Thread mainThread = new Thread(StartWork);
      //mainThread.Start();





      /*
        To do list:
      1)Сделать дозапись файлов | галочка
      2)Сделать так, чтобы сохранялось несколько картинок с одного поста | галочка
      3)Потоки, потоки и еще раз потоки...
      4)Сделать так, чтобы потоки сами обновляли свой список постов при скроле новостей вниз | а надо ли?
      */


      //WriteIdAndText(postsWithText);
      //WriteIdPostAndPicture(postsWithPicture);
      //WriteIdAndHrefPicture(postsWithPicture);
    }
  }
}
