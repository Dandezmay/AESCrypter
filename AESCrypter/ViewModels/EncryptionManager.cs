using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using AESCrypter.Models;
using AES_EnDecryptor.Basement;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace AESCrypter.ViewModels
{
    class EncryptionManager : ObservableObject
    {
        private const int _block_size = 16;

        private int _skip_size;

        private bool _skip_scheme_option;

        private bool _option_scheme;

        private bool _delete_source_file_option;

        private string _import_file_path;

        private string _export_directory_path;

        private string _aes_key;

        private SkipProportion _selected_skip_proportion;

        public bool SkipSchemeOption
        {
            get => _skip_scheme_option;
            set
            {
                _skip_scheme_option = value;
                if (_skip_scheme_option) _skip_size = SelectedSkipProportion.Multiple * _block_size;
                else _skip_size = 0;
                RaisePropertyChanged("SkipSchemeOption");
            }
        }

        public bool OptionScheme
        {
            get => _option_scheme;
            set
            {
                _option_scheme = value;
                RaisePropertyChanged(("OptionScheme"));
            }
        }

        public bool DeleteSourceFileOption
        {
            get => _delete_source_file_option;
            set
            {
                _delete_source_file_option = value;
                RaisePropertyChanged("DeleteSourceFileOption");
            }
        }

        public string ImportFilePath
        {
            get => _import_file_path;
            set
            {
                _import_file_path = value;
                RaisePropertyChanged("ImportFilePath");
            }
        }

        public string ExportDirectoryPath
        {
            get => _export_directory_path;
            set
            {
                _export_directory_path = value;
                RaisePropertyChanged("ExportDirectoryPath");
            }
        }

        public string AESKey
        {
            get => _aes_key;
            set
            {
                _aes_key = value;
                RaisePropertyChanged("AESKey");
            }
        }

        public SkipProportion SelectedSkipProportion
        {
            get => _selected_skip_proportion;
            set
            {
                _selected_skip_proportion = value;
                _skip_size = _selected_skip_proportion.Multiple * _block_size;
                RaisePropertyChanged("SelectedSkipProportion");
            }
        }

        public SkipProportion[] SkipProportionList => AES.SkipProportionListSource;
        
        public EncryptionManager()
        {
            _skip_scheme_option = true;
            _option_scheme = true;
            _delete_source_file_option = false;
            SelectedSkipProportion = SkipProportionList[1];
        }

        private void SelectInputFile(object parameter)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            //设置这个对话框的起始打开路径
            //ofd.InitialDirectory = @"C:\";
            //设置打开的文件的类型，注意过滤器的语法
            ofd.Filter = "所有文件|*.*";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (ofd.ShowDialog() == true)
            {
                string filePath = ofd.FileName;
                ImportFilePath = filePath;
                ExportDirectoryPath = filePath.Replace(ofd.SafeFileName, "");
            }
        }

        private void SelectOutputPath(object parameter)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim();
            ExportDirectoryPath = m_Dir;
        }

        private void EncryptFile(string source, string output, string password)
        {
            try
            {
                using (FileStream fsr = new FileStream(source, FileMode.Open))
                {
                    using (FileStream fsw = new FileStream(output + "\\" + Security.ToAESEncrypt(Path.GetFileName(source), password), FileMode.Create))
                    {
                        var length = fsr.Length;
                        var readsize = 0;
                        var delta = (_skip_size + _block_size) * 1.0F / length;
                        var totalpercent = 0.0;

                        AES aes = new AES(AES.KeySize.Bits256, Security.GetKey(password));
                        byte[] buffer = new byte[_block_size];
                        byte[] newbuffer = new byte[_block_size];
                        readsize = fsr.Read(buffer, 0, _block_size);
                        while (readsize != 0)
                        {
                            //Encrypt Para
                            aes.Cipher(buffer, newbuffer);
                            fsw.Write(newbuffer, 0, readsize);
                            //Skip Para
                            buffer = new byte[_skip_size];
                            readsize = fsr.Read(buffer, 0, _skip_size);
                            if (readsize == 0 && _skip_size != 0)
                                break;
                            fsw.Write(buffer, 0, readsize);
                            //Read New Para
                            buffer = new byte[_block_size];
                            readsize = fsr.Read(buffer, 0, _block_size);
                            if (readsize == 0)
                                break;
                            totalpercent += delta;
                        }
                    }
                }
                if (DeleteSourceFileOption) File.Delete(source);
                System.Diagnostics.Process.Start("explorer.exe ", Path.GetDirectoryName(output));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Encrypt Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DecryptFile(string source, string output, string password)
        {
            try
            {
                using (FileStream fsr = new FileStream(source, FileMode.Open))
                {
                    using (FileStream fsw = new FileStream(output + "\\(decrypted)" + Security.ToAESDecrypt(Path.GetFileName(source), password), FileMode.Create))
                    {
                        var length = fsr.Length;
                        var readsize = 0;
                        var delta = (_skip_size + _block_size) * 1.0F / length;
                        var totalpercent = 0.0;

                        AES aes = new AES(AES.KeySize.Bits256, Security.GetKey(password));
                        byte[] buffer = new byte[_block_size];
                        byte[] newbuffer = new byte[_block_size];
                        readsize = fsr.Read(buffer, 0, _block_size);
                        while (readsize != 0)
                        {
                            //Encrypt Para
                            aes.InvCipher(buffer, newbuffer);
                            fsw.Write(newbuffer, 0, readsize);
                            //Skip Para
                            buffer = new byte[_skip_size];
                            readsize = fsr.Read(buffer, 0, _skip_size);
                            if (readsize == 0 && _skip_size != 0)
                                break;
                            fsw.Write(buffer, 0, readsize);
                            //Read New Para
                            buffer = new byte[_block_size];
                            readsize = fsr.Read(buffer, 0, _block_size);
                            if (readsize == 0)
                                break;
                            totalpercent += delta;
                        }
                    }
                }
                if (DeleteSourceFileOption) File.Delete(source);
                System.Diagnostics.Process.Start("explorer.exe ", Path.GetDirectoryName(output));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Decrypt Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public ICommand SelectInputFileCommand => new DelegateCommand(SelectInputFile);

        public ICommand SelectOutputPathCommand => new DelegateCommand(SelectOutputPath);

        public ICommand ExecuteOperationCommand => new DelegateCommand((obj) =>
        {
            if (OptionScheme) EncryptFile(ImportFilePath, ExportDirectoryPath, AESKey);
            else DecryptFile(ImportFilePath, ExportDirectoryPath, AESKey);
        });

    }
}
