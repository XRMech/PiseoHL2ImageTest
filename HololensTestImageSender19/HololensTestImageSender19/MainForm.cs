using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Security.Policy;
using System.Windows.Forms;

namespace HololensTestImageSender19
{

        public partial class MainForm : Form
        {
            private string _selectedImagePath = string.Empty;
            private readonly HttpClient _client = new HttpClient();  // Single HttpClient instance
            private bool isSending = false;

            public MainForm()
            {
                InitializeComponent();
                ipTextBox.Text = Properties.Settings.Default.ipAddress;
                portTextBox.Text = Properties.Settings.Default.port;
            }

            private void browseButton_Click(object sender, EventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Select an Image"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _selectedImagePath = openFileDialog.FileName;

                    // Display the selected image in the PictureBox
                    using (var imgStream = new FileStream(_selectedImagePath, FileMode.Open, FileAccess.Read))
                    {
                        pictureBox.Image = Image.FromStream(imgStream);
                    }

                }
            }


            private async void sendButton_Click(object sender, EventArgs e)
            {
                if (isSending)
                {
                    MessageBox.Show("Currently sending an image. Please wait.");
                    return;
                }

                if (string.IsNullOrEmpty(_selectedImagePath) ||
                    string.IsNullOrEmpty(ipTextBox.Text) ||
                    string.IsNullOrEmpty(portTextBox.Text))
                {
                    MessageBox.Show("Ensure IP, port, and image are selected!");
                    return;
                }

                if (!Uri.TryCreate($"http://{ipTextBox.Text}:{portTextBox.Text}", UriKind.Absolute, out var url))
                {
                    MessageBox.Show("Invalid IP or port format.");
                    return;
                }
                // Save the current IP and port to settings
                Properties.Settings.Default.ipAddress = ipTextBox.Text;
                Properties.Settings.Default.port = portTextBox.Text;
                Properties.Settings.Default.Save();
                isSending = true;  // Prevent sending another image simultaneously


                try
                {
                    richTextBox.AppendText("Connecting...\n");

                    using (var content = new MultipartFormDataContent())
                    using (var fileStream = new System.IO.FileStream(_selectedImagePath, System.IO.FileMode.Open))
                    {
                        content.Add(new StreamContent(fileStream), "file", System.IO.Path.GetFileName(_selectedImagePath));
                        var response = await _client.PostAsync(url, content);

                        if (response.IsSuccessStatusCode)
                        {
                            richTextBox.AppendText("Image sent successfully!\n");
                        }
                        else
                        {
                            richTextBox.AppendText($"Failed to send image. Response: {url}, {response.StatusCode} {response.ReasonPhrase}\n");
                        }
                    }
                }
                catch (HttpRequestException hre)
                {
                    richTextBox.AppendText($"Network error: {hre.Message}\n");
                }
                catch (Exception ex)
                {
                    richTextBox.AppendText($"An error occurred: {ex.Message}\n");
                }
                finally
                {
                    isSending = false;
                }
            }

            private async void clearTextureButton_Click(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(ipTextBox.Text) || string.IsNullOrEmpty(portTextBox.Text))
                {
                    MessageBox.Show("Ensure IP and port are entered!");
                    return;
                }

                if (!Uri.TryCreate($"http://{ipTextBox.Text}:{portTextBox.Text}/clearTexture", UriKind.Absolute, out var url))
                {
                    MessageBox.Show("Invalid IP or port format.");
                    return;
                }

                try
                {
                    richTextBox.AppendText("Sending clear texture command...\n");

                    // Send a POST request to the server to clear the texture.
                    var response = await _client.PostAsync(url, null); // null content signifies we're not sending data in this request

                    if (response.IsSuccessStatusCode)
                    {
                        richTextBox.AppendText("Texture cleared successfully!\n");
                    }
                    else
                    {
                        richTextBox.AppendText($"Failed to clear texture. Response: {url}, {response.StatusCode} {response.ReasonPhrase}\n");
                    }
                }
                catch (HttpRequestException hre)
                {
                    richTextBox.AppendText($"Network error: {hre.Message}\n");
                }
                catch (Exception ex)
                {
                    richTextBox.AppendText($"An error occurred: {ex.Message}\n");
                }
            }


            // Ensure HttpClient is disposed when the form is closing
            private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
            {
                _client.Dispose();
            }
        }
    }