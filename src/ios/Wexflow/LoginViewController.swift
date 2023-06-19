//
//  LoginViewController.swift
//  Wexflow
//
//  Created by Akram El Assas on 28/08/2019.
//  Copyright © 2019 Akram El Assas. All rights reserved.
//

import UIKit

//@available(iOS, deprecated: 13.0)
class LoginViewController: UIViewController, ObservableObject {

    static var Username = ""
    static var Password = ""
    
    @IBOutlet weak var usernameTextField: UITextField!
    @IBOutlet weak var passwordTextField: UITextField!
    
    var WexflowServerUrl: String = ""
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        if let url = UserDefaults.standard.string(forKey: "wexflow_server_url_preference") {
            self.WexflowServerUrl = cleanupUrl(url: url)
        }//else{
        //    self.WexflowServerUrl = "http://192.168.0.198:8000/api/v1"
        //}
        print("LoginViewController.WexflowServerUrl: " + self.WexflowServerUrl)
        usernameTextField.text = ""
        passwordTextField.text = ""
        
        /*UserDefaults.standard.addObserver(self, forKeyPath: "wexflow_server_url_preference", options:NSKeyValueObservingOptions.new, context: nil)*/
        
        NotificationCenter.default.addObserver(self, selector: #selector(LoginViewController.defaultsChanged(notification:)), name: UserDefaults.didChangeNotification, object: nil)
    }

    /*deinit {
        NotificationCenter.default.removeObserver(self)
    }*/
    
    func registerSettingsBundle(){
        let appDefaults = [String:AnyObject]()
        UserDefaults.standard.register(defaults: appDefaults)
    }
    
   @objc func defaultsChanged(notification: Notification){
        if let url = UserDefaults.standard.string(forKey: "wexflow_server_url_preference") {
            self.WexflowServerUrl = cleanupUrl(url: url)
            print("LoginViewController.WexflowServerUrl(changed): " + self.WexflowServerUrl)
        }
    }
    
    override func didReceiveMemoryWarning() {
        super.didReceiveMemoryWarning()
        // Dispose of any resources that can be recreated.
    }
    
    @IBAction func onSigninClick(_ sender: UIButton) {
        
        if usernameTextField.text != nil && !(usernameTextField.text?.isEmpty)! && passwordTextField.text != nil && !(passwordTextField.text?.isEmpty)! {
        
            let pass = self.md5(string: self.passwordTextField.text!)
            
            let url = URL(string: WexflowServerUrl + "user?username=" + usernameTextField.text!)
            let session = URLSession.shared
            let request = NSMutableURLRequest(url: url! as URL)
            request.httpMethod = "GET"
            let auth = "Basic " + LoginViewController.toBase64(str: usernameTextField.text! + ":" + pass)
            request.setValue(auth, forHTTPHeaderField: "Authorization")
            
            session.dataTask(with: request as URLRequest) { (data, _, error) in
                if error != nil {
                    //print(error!)
                    DispatchQueue.main.async{
                        self.toast(message: "Network error.")
                    }
                    return
                }
                
                if data != nil && !data!.isEmpty {
                    let jsonResponse = try! JSONSerialization.jsonObject(with: data!, options: [])
                    let user = jsonResponse as? [String: Any]
                    let username = user!["Username"] as! String
                    let password = user!["Password"] as! String
                    let userProfile = user!["UserProfile"] as! Int

                    if (userProfile == 0 || userProfile == 1) && pass == password {
                        LoginViewController.Username = username
                        LoginViewController.Password = password
                        DispatchQueue.main.async{
                            let storyBoard: UIStoryboard = UIStoryboard(name: "Main", bundle: nil)
                            let viewController = storyBoard.instantiateViewController(withIdentifier: "ViewController") as! ViewController
                            self.present(viewController, animated: true, completion: nil)
                        }
                    }else{
                        
                        if userProfile == 2{
                            DispatchQueue.main.async{
                                self.toast(message: "Access denied.")
                            }
                        }else{
                            DispatchQueue.main.async{
                                self.toast(message: "Wrong credentials.")
                            }
                        }
                        
                       
                    }
                }else{
                    DispatchQueue.main.async{
                        self.toast(message: "Wrong credentials.")
                    }
                }
                
                
            }.resume()
            
            
        }else{
            self.toast(message: "Empty credentials.")
        }
        
       
        
    }

    @IBAction func onSettingsClick(_ sender: UIButton) {
        let settings_app: URL = URL(string: UIApplication.openSettingsURLString)!
        UIApplication.shared.open(settings_app)
    }
    
    static func toBase64(str: String) -> String{
        let data = (str).data(using: String.Encoding.utf8)
        let base64 = data!.base64EncodedString(options: NSData.Base64EncodingOptions(rawValue: 0))
        let res = base64.replacingOccurrences(of: "\n", with: "")
        return res
    }
        
    func md5(string: String) -> String {
        let data = string.data(using:.utf8)!
        let hash = data.withUnsafeBytes { (bytes: UnsafeRawBufferPointer) -> [UInt8] in
            var hash = [UInt8](repeating: 0, count: Int(CC_MD5_DIGEST_LENGTH))
            CC_MD5(bytes.baseAddress, CC_LONG(data.count), &hash)
            return hash
        }
        return hash.map { String(format: "%02x", $0) }.joined()
    }
    
    /*
    // MARK: - Navigation

    // In a storyboard-based application, you will often want to do a little preparation before navigation
    override func prepare(for segue: UIStoryboardSegue, sender: Any?) {
        // Get the new view controller using segue.destinationViewController.
        // Pass the selected object to the new view controller.
    }
    */

}
