using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace demo
{
    /// <summary>
    /// 可独立使用或用于导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// 在此页将要在 Frame 中显示时进行调用。
        /// </summary>
        /// <param name="e">描述如何访问此页的事件数据。
        /// 此参数通常用于配置页。</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: 准备此处显示的页面。

            // TODO: 如果您的应用程序包含多个页面，请确保
            // 通过注册以下事件来处理硬件“后退”按钮:
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed 事件。
            // 如果使用由某些模板提供的 NavigationHelper，
            // 则系统会为您处理该事件。
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.RadioButton myRadioButton = ((Windows.UI.Xaml.Controls.RadioButton)sender);
            string myLayerNameTag = System.Convert.ToString(myRadioButton.Tag);
            foreach (Esri.ArcGISRuntime.Layers.Layer oneLayer in Map1.Layers)
            {
                if (oneLayer is Esri.ArcGISRuntime.Layers.BingLayer)
                {
                    oneLayer.IsVisible = false;
                }
            }

            // Turn on the visibility of the BingLayer that matches the RadioButton that was just clicked.
            Esri.ArcGISRuntime.Layers.LayerCollection myLayerCollection = Map1.Layers;
            Esri.ArcGISRuntime.Layers.BingLayer myBingLayer = (Esri.ArcGISRuntime.Layers.BingLayer)myLayerCollection[myLayerNameTag];
            myBingLayer.IsVisible = true;

        }

        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Windows.UI.Xaml.Controls.TextBox myTextBox = (sender as Windows.UI.Xaml.Controls.TextBox);
            if ((myTextBox.Text.Length) >= 64)
            {
                LoadMapButton.IsEnabled = true;
            }
            else
            {
                LoadMapButton.IsEnabled = false;
            }

        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Uri uri = new System.Uri("https://www.bingmapsportal.com");
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

        }

        private async void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            System.Net.Http.HttpClient myHttpClient = new System.Net.Http.HttpClient();

            // Construct a Url with the Bing Key as a argument pair to contact the Microsoft development Bing web server.   
            string myUri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            System.Net.Http.HttpResponseMessage response = await myHttpClient.GetAsync(new System.Uri(myUri));
            try
            {
                // Get the response stream. 
                var stream = await response.Content.ReadAsStreamAsync();

                // If error code then this will throw an exception.
                response.EnsureSuccessStatusCode();

                // Create a new DataContractJsonSerializer using our custom BingAuthentication DataContract defined at the end of this class file. 
                System.Runtime.Serialization.Json.DataContractJsonSerializer mySerializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(BingAuthentication));

                // Set the result of the DataContractJsonSerializer.ReadObject to our custom BigAuthentication DataContract.
                BingAuthentication myBingAuthentication = mySerializer.ReadObject(stream) as BingAuthentication;

                // Get the BingAuthentication.AuthenticationResultCode string from the DataContract.  
                string myAuthenticationResult = myBingAuthentication.AuthenticationResultCode.ToString();

                // Test if the BingAuthentication.AuthenticationResultCode equals the string 'ValidCredentials'  
                if (myAuthenticationResult == "ValidCredentials")
                {
                    // We did get back the string 'ValidCredentials' from the HttpClient request to the development Microsoft Bing web site.  

                    // Get the Enumerations of BingLayer.MapStyle types of Bing maps that are available from Microsoft.
                    System.Array myBingLayerTypes = System.Enum.GetValues(typeof(Esri.ArcGISRuntime.Layers.BingLayer.LayerType));
                    int[] myLayerTypes = (int[])myBingLayerTypes;

                    // Loop through each BingLayer.MapStyle enumeration value (an Integer).   
                    foreach (Esri.ArcGISRuntime.Layers.BingLayer.LayerType myLayerType in myLayerTypes)
                    {
                        // Create a new BingLayer and set the various properties (.ID, .MapStyle, .Key, and .IsVisible)
                        Esri.ArcGISRuntime.Layers.BingLayer myBingLayer = new Esri.ArcGISRuntime.Layers.BingLayer();
                        myBingLayer.ID = myLayerType.ToString();
                        myBingLayer.MapStyle = myLayerType;
                        myBingLayer.Key = BingKeyTextBox.Text;
                        myBingLayer.IsVisible = false;

                        // Add each of the different Bing maps types to the Map's LayerCollection.
                        Map1.Layers.Add(myBingLayer);
                    }

                    // Make the first BingLayer in the BingLayer() array be the one that is visible to the user.
                    Map1.Layers[0].IsVisible = true;

                    // Hide the controls that the user has to enter the Bing Key.
                    BingKeyGrid.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    InvalidBingKeyTextBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    // Show the RadioButton controls that allow the user to switch between viewing the different BingLayer types.
                    LayerStyleGrid.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
            catch (System.Exception)
            {
                // There WAS an error returned from the HttpClient.OpenReadCompleted call.   
                // Keep the set of controls prompting the user for a valid Bing Key.

                InvalidBingKeyTextBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        // Create a custom BingAuthentication class that can be used to hold JSON serializable information from the HttpClient. We will use this to see if  
        // we get back the string 'ValidCredentials' from a Microsoft Bing development web server for a specific Bing Key provided by the user.
        [System.Runtime.Serialization.DataContract]
        public class BingAuthentication
        {
            [System.Runtime.Serialization.DataMember(Name = "authenticationResultCode")]
            public string AuthenticationResultCode { get; set; }
        }

        }
    }
