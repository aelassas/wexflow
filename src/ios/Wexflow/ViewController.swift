//
//  ViewController.swift
//  Wexflow
//
//  Created by Akram El Assas on 27/08/2019.
//  Copyright © 2019 Akram El Assas. All rights reserved.
//

import UIKit

class ViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    
    @IBOutlet weak var tableView: UITableView!
    @IBOutlet weak var startButton: UIButton!
    @IBOutlet weak var SuspendButton: UIButton!
    @IBOutlet weak var ResumeButton: UIButton!
    @IBOutlet weak var StopButton: UIButton!
    @IBOutlet weak var DisapproveButton: UIButton!
    @IBOutlet weak var ApproveButton: UIButton!
    @IBOutlet weak var infoLabel: UILabel!
    
    var WexflowServerUrl: String = ""
    let textCellIdentifier = "TextCell"
    let selectedBackgroundColor = UIColor(red: 134/255, green: 211/255, blue: 246/255, alpha: 1.0)
    let selectedTextColor = UIColor(red: 0/255, green: 0/255, blue: 0/255, alpha: 1.0)
    let unselectedBackgroundColor = UIColor(red: 27/255, green: 27/255, blue: 27/255, alpha: 1.0)
    let unselectedTextColor = UIColor(red: 221/255, green: 221/255, blue: 221/255, alpha: 1.0)
    
    var workflows = [Workflow]()
    var workflowId = -1
    var selectedIndex = -1
    var previousSelectedCell:WorkflowTableViewCell?
    var timer:Timer?
    var jobs:[Int:String] = [:]
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        if let url = UserDefaults.standard.string(forKey: "wexflow_server_url_preference") {
            self.WexflowServerUrl = cleanupUrl(url: url)
        }//else{
        //    self.WexflowServerUrl = "http://192.168.0.198:8000/api/v1"
        //}
        print("ViewController.WexflowServerUrl: " + self.WexflowServerUrl)
        
        self.startButton.isEnabled = false
        self.SuspendButton.isEnabled = false
        self.ResumeButton.isEnabled = false
        self.StopButton.isEnabled = false
        self.ApproveButton.isEnabled = false
        self.DisapproveButton.isEnabled = false
        
        NotificationCenter.default.addObserver(self, selector: #selector(ViewController.defaultsChanged(notification:)), name: UserDefaults.didChangeNotification, object: nil)
        
        tableView.delegate = self
        tableView.dataSource = self
        
        loadWorkflows()
    }
    
    func registerSettingsBundle(){
        let appDefaults = [String:AnyObject]()
        UserDefaults.standard.register(defaults: appDefaults)
    }
    
    @objc func defaultsChanged(notification: Notification){
        if let url = UserDefaults.standard.string(forKey: "wexflow_server_url_preference") {
            self.WexflowServerUrl = cleanupUrl(url: url)
            print("ViewController.WexflowServerUrl(changed): " + self.WexflowServerUrl)
        }
    }
    
    /*deinit { //Not needed for iOS9 and above. ARC deals with the observer in higher versions.
        NotificationCenter.default.removeObserver(self)
    }*/
    
    @IBAction func onSettingsClick(_ sender: UIButton) {
        let settings_app: URL = URL(string: UIApplication.openSettingsURLString)!
        UIApplication.shared.open(settings_app)
    }
    
    func numberOfSectionsInTableView(tableView: UITableView) -> Int {
        return 1
    }
    
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return workflows.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: textCellIdentifier, for: indexPath) as! WorkflowTableViewCell
        
        let row = indexPath.row
        let workflow = workflows[row]
        
        cell.idLabel.text = String(workflow.id)
        cell.nameLabel.text = workflow.name
        cell.launchTypeLabel.text = launchTypeToString(launchType: workflow.launchType)
        cell.selectionStyle = .none
        
        if indexPath.row == self.selectedIndex {
            cell.contentView.backgroundColor = selectedBackgroundColor
            cell.idLabel.textColor = selectedTextColor
            cell.nameLabel.textColor = selectedTextColor
            cell.launchTypeLabel.textColor = selectedTextColor
        }else{
            cell.contentView.backgroundColor = unselectedBackgroundColor
            cell.idLabel.textColor = unselectedTextColor
            cell.nameLabel.textColor = unselectedTextColor
            cell.launchTypeLabel.textColor = unselectedTextColor
        }
        
        return cell
    }
    
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        let cell = tableView.cellForRow(at: indexPath) as! WorkflowTableViewCell
        cell.contentView.backgroundColor = selectedBackgroundColor
        cell.idLabel.textColor = selectedTextColor
        cell.nameLabel.textColor = selectedTextColor
        cell.launchTypeLabel.textColor = selectedTextColor
        
        self.selectedIndex = indexPath.row
        let workflow = self.workflows[self.selectedIndex]
        
        if previousSelectedCell != nil && workflow.id != self.workflowId{
            self.previousSelectedCell!.contentView.backgroundColor = unselectedBackgroundColor
            self.previousSelectedCell!.idLabel.textColor = unselectedTextColor
            self.previousSelectedCell!.nameLabel.textColor = unselectedTextColor
            self.previousSelectedCell!.launchTypeLabel.textColor = unselectedTextColor
        }
 
        previousSelectedCell = cell
        
        if workflow.id != self.workflowId{
            tableView.deselectRow(at: indexPath, animated: true)
        }
        
        self.workflowId = workflow.id
        
        if self.timer != nil{
            self.timer?.invalidate()
            self.timer = nil
        }
        
        if !workflow.isEnabled{
            updateButtons(force: true)
        }else{
            self.timer = Timer.scheduledTimer(timeInterval: 0.5, target: self, selector: #selector(timerAction), userInfo: nil, repeats: true)
            
            updateButtons(force: true)
        }
    }
    
    @objc func timerAction() {
        self.updateButtons(force: false)
    }
    
    func updateButtons(force: Bool){
        if self.workflowId > -1 {
            let url = URL(string: WexflowServerUrl + "workflow?w=" + String(self.workflowId))
            let request = NSMutableURLRequest(url: url! as URL)
            request.httpMethod = "GET"
            let auth = "Basic " + LoginViewController.toBase64(str: LoginViewController.Username + ":" + LoginViewController.Password)
            request.setValue(auth, forHTTPHeaderField: "Authorization")
            
            URLSession.shared.dataTask(with: request as URLRequest) { (data, _, error) in
                if error != nil {
                    //print(error!)
                    DispatchQueue.main.async{
                        self.toast(message: "Network error.")
                    }
                    return
                }
                let jsonResponse = try! JSONSerialization.jsonObject(with: data!, options: [])
                let workflow = jsonResponse as? [String: Any]
                let id = workflow!["Id"] as! Int
                let instanceId = workflow!["InstanceId"] as! String
                let name = workflow!["Name"] as! String
                let launchType = LaunchType(rawValue: (workflow!["LaunchType"] as? Int)!)
                let isEnabled = workflow!["IsEnabled"] as! Bool
                let isApproval = workflow!["IsApproval"] as! Bool
                let isWaitingForApproval = workflow!["IsWaitingForApproval"] as! Bool
                let isRunning = workflow!["IsRunning"] as! Bool
                let isPaused = workflow!["IsPaused"] as! Bool
                
                let wf = Workflow(id: id, instanceId: instanceId, name: name, launchType: launchType!, isEnabled: isEnabled, isApproval: isApproval, isWaitingForApproval: isWaitingForApproval, isRunning: isRunning, isPaused: isPaused)
                
                DispatchQueue.main.async{
                    if !wf.isEnabled {
                        self.infoLabel.text = "This workflow is disabled."
                        self.startButton.isEnabled = false
                        self.SuspendButton.isEnabled = false
                        self.ResumeButton.isEnabled = false
                        self.StopButton.isEnabled = false
                        self.ApproveButton.isEnabled = false
                        self.DisapproveButton.isEnabled = false
                    }else{
                        let changed = self.workflows[self.selectedIndex].isRunning != wf.isRunning || self.workflows[self.selectedIndex].isPaused != wf.isPaused || self.workflows[self.selectedIndex].isWaitingForApproval != wf.isWaitingForApproval
                        
                        self.workflows[self.selectedIndex].isRunning = wf.isRunning
                        self.workflows[self.selectedIndex].isPaused = wf.isPaused
                        self.workflows[self.selectedIndex].isWaitingForApproval = wf.isWaitingForApproval
                        
                        if !force && !changed{
                            return
                        }
                        self.startButton.isEnabled = !wf.isRunning
                        self.SuspendButton.isEnabled = wf.isRunning && !wf.isPaused
                        self.ResumeButton.isEnabled = wf.isPaused
                        self.StopButton.isEnabled = wf.isRunning && !wf.isPaused
                        self.ApproveButton.isEnabled = wf.isApproval && wf.isWaitingForApproval
                        self.DisapproveButton.isEnabled = wf.isApproval && wf.isWaitingForApproval
                        
                        if wf.isApproval && wf.isWaitingForApproval && !wf.isPaused {
                            self.infoLabel.text = "This workflow is waiting for approval..."
                        }
                        else{
                            if wf.isRunning && !wf.isPaused {
                                self.infoLabel.text = "This workflow is running..."
                            }
                            else if wf.isPaused {
                                self.infoLabel.text = "This workflow is suspended."
                            }
                            else {
                                self.infoLabel.text = ""
                            }
                        }
                        
                        if wf.isRunning{
                            self.jobs[wf.id] = wf.instanceId
                        }
                        
                    }
                }
                
                
                
                }.resume()
        }
    }
    
    //this function is fetching the json from URL
    func loadWorkflows(){
        self.workflows.removeAll()
        let url = URL(string: WexflowServerUrl + "search?s=")
        let request = NSMutableURLRequest(url: url! as URL)
        request.httpMethod = "GET"
        let auth = "Basic " + LoginViewController.toBase64(str: LoginViewController.Username + ":" + LoginViewController.Password)
        request.setValue(auth, forHTTPHeaderField: "Authorization")
        
        URLSession.shared.dataTask(with: request as URLRequest) { (data, _, error) in
            if error != nil {
                //print(error!)
                DispatchQueue.main.async{
                    self.toast(message: "Network error.")
                }
                return
            }
            
            let jsonResponse = try! JSONSerialization.jsonObject(with: data!, options: [])
            let jsonArray = jsonResponse as? [[String: Any]]
            
            if jsonArray != nil {
                for workflow in jsonArray! {
                    let id = workflow["Id"] as! Int
                    let instanceId = workflow["InstanceId"] as! String
                    let name = workflow["Name"] as! String
                    let launchType = LaunchType(rawValue: (workflow["LaunchType"] as? Int)!)
                    let isEnabled = workflow["IsEnabled"] as! Bool
                    let isApproval = workflow["IsApproval"] as! Bool
                    let isWaitingForApproval = workflow["IsWaitingForApproval"] as! Bool
                    let isRunning = workflow["IsRunning"] as! Bool
                    let isPaused = workflow["IsPaused"] as! Bool
                    
                    self.workflows.append(Workflow(id: id, instanceId: instanceId, name: name, launchType: launchType!, isEnabled: isEnabled, isApproval: isApproval, isWaitingForApproval: isWaitingForApproval, isRunning: isRunning, isPaused: isPaused))
                    self.workflows.sort(by: { $0.id < $1.id })
                }
            }else{
                DispatchQueue.main.async{
                    self.toast(message: "Error while loading.")
                }
            }
            
            DispatchQueue.main.async{
                self.tableView.reloadData()
                self.tableView.scroll(to: .top, animated: true)
            }
            
            }.resume()
    }
    
    func launchTypeToString(launchType: LaunchType) -> String{
        switch (launchType) {
        case .Startup:
            return "Startup"
            
        case .Trigger:
            return "Trigger"
            
        case .Periodic:
            return "Periodic"
            
        case .Cron:
            return "Cron"
            
        }
        
    }
    
    func post(url: URL, message: String, hasResult: Bool, workflowId: Int){
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        let auth = "Basic " + LoginViewController.toBase64(str: LoginViewController.Username + ":" + LoginViewController.Password)
        request.setValue(auth, forHTTPHeaderField: "Authorization")
        
        request.setValue("close", forHTTPHeaderField: "Connection")
        
        URLSession.shared.dataTask(with: request) { (data, response, error) in
            if error != nil {
                //print(error!)
                DispatchQueue.main.async{
                    self.toast(message: "Network error.")
                }
                return
            }

            if hasResult {
                let response = String(data: data!, encoding: .utf8)
                
                if response == "true" || response == "false" {
                  let result = response?.bool
                  if result! {
                      DispatchQueue.main.async{
                          self.toast(message: message)
                      }
                  }else{
                      DispatchQueue.main.async{
                          self.toast(message: "Not supported.")
                      }
                  }
                }else if !response!.isEmpty{
                    self.jobs[workflowId] = response!.replacingOccurrences(of: "\"", with: "")
                }
            }else{
                DispatchQueue.main.async{
                    self.toast(message: message)
                }
            }
            }.resume()
    }
    
    @IBAction func onStartClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "start?w=" + String(workflowId))
            let message = "Workflow " + String(workflowId) + " started."
            post(url: url!, message: message, hasResult: true, workflowId: workflowId)
        }
    }
    
    @IBAction func onSuspendClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "suspend?w=" + String(workflowId) + "&i=" + jobs[workflowId]!)
            let message = "Workflow " + String(workflowId) + " suspended."
            post(url: url!, message: message, hasResult: true, workflowId: workflowId)
        }
    }
    
    @IBAction func onResumeClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "resume?w=" + String(workflowId) + "&i=" + jobs[workflowId]!)
            let message =  "Workflow " + String(workflowId) + " resumed."
            post(url: url!, message: message, hasResult: false, workflowId: workflowId)
        }
    }
    
    @IBAction func onStopClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "stop?w=" + String(workflowId) + "&i=" + jobs[workflowId]!)
            let message = "Workflow " + String(workflowId) + " stopped."
            post(url: url!, message: message, hasResult: true, workflowId: workflowId)
        }
    }
    
    @IBAction func onApproveClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "approve?w=" + String(workflowId) + "&i=" + jobs[workflowId]!)
            let message = "Workflow " + String(workflowId) + " approved."
            post(url: url!, message: message, hasResult: false, workflowId: workflowId)
        }
    }
    
    @IBAction func onDisapproveClick(_ sender: UIButton) {
        if workflowId != -1 {
            let url = URL(string: WexflowServerUrl + "reject?w=" + String(workflowId) + "&i=" + jobs[workflowId]!)
            let message = "Workflow " + String(workflowId) + " rejected."
            post(url: url!, message: message, hasResult: false, workflowId: workflowId)
        }
    }
    
    @IBAction func onRefreshClick(_ sender: UIButton) {
        self.selectedIndex = -1
        self.workflowId = -1
        self.previousSelectedCell = nil
        self.startButton.isEnabled = false
        self.SuspendButton.isEnabled = false
        self.ResumeButton.isEnabled = false
        self.StopButton.isEnabled = false
        self.ApproveButton.isEnabled = false
        self.DisapproveButton.isEnabled = false
        self.infoLabel.text = ""
        if self.timer != nil{
            self.timer?.invalidate()
            self.timer = nil
        }
        loadWorkflows()
    }
    
}

enum LaunchType: Int {
    case Startup = 0
    case Trigger = 1
    case Periodic = 2
    case Cron = 3
}

struct Workflow{
    
    let id: Int
    let instanceId: String
    let name: String
    let launchType: LaunchType
    let isEnabled: Bool
    let isApproval:Bool
    var isWaitingForApproval: Bool
    var isRunning:Bool
    var isPaused:Bool
    
    public init(id: Int, instanceId: String, name: String, launchType: LaunchType, isEnabled: Bool, isApproval: Bool, isWaitingForApproval: Bool, isRunning: Bool, isPaused: Bool) {
        
        self.id = id
        self.instanceId = instanceId
        self.name = name
        self.launchType = launchType
        self.isEnabled = isEnabled
        self.isApproval = isApproval
        self.isWaitingForApproval = isWaitingForApproval
        self.isRunning = isRunning
        self.isPaused = isPaused
        
    }
    
}

extension UITableView {
    
    public func reloadData(_ completion: @escaping ()->()) {
        UIView.animate(withDuration: 0, animations: {
            self.reloadData()
        }, completion:{ _ in
            completion()
        })
    }
    
    func scroll(to: scrollsTo, animated: Bool) {
        DispatchQueue.main.asyncAfter(deadline: .now() + .milliseconds(300)) {
            let numberOfSections = self.numberOfSections
            let numberOfRows = self.numberOfRows(inSection: numberOfSections-1)
            switch to{
            case .top:
                if numberOfRows > 0 {
                    let indexPath = IndexPath(row: 0, section: 0)
                    self.scrollToRow(at: indexPath, at: .top, animated: animated)
                }
            case .bottom:
                if numberOfRows > 0 {
                    let indexPath = IndexPath(row: numberOfRows-1, section: (numberOfSections-1))
                    self.scrollToRow(at: indexPath, at: .bottom, animated: animated)
                }
            }
        }
    }
    
    enum scrollsTo {
        case top,bottom
    }
}

extension UIViewController {
    
    func cleanupUrl(url: String) -> String{
        let regex = try! NSRegularExpression(pattern: "/+$", options: NSRegularExpression.Options.caseInsensitive)
        let range = NSRange(location: 0, length: url.count)
        let cleanUrl = regex.stringByReplacingMatches(in: url, options: [], range: range, withTemplate: "")
        return cleanUrl + "/"
    }
    
    func toast(message : String) {
        let font = UIFont(name: "IranSansMobile", size: 17)
        let toastLabel = UILabel(frame: CGRect(x: self.view.frame.size.width/2 - 75, y: self.view.frame.size.height-100, width: 230, height: 35))
        toastLabel.backgroundColor = UIColor.black.withAlphaComponent(0.6)
        toastLabel.textColor = UIColor.white
        toastLabel.font = font
        toastLabel.textAlignment = .center
        toastLabel.text = message
        toastLabel.alpha = 1.0
        toastLabel.layer.cornerRadius = 10
        toastLabel.clipsToBounds  =  true
        self.view.addSubview(toastLabel)
        UIView.animate(withDuration: 5.0, delay: 0.1, options: .curveEaseOut, animations: {
            toastLabel.alpha = 0.0
        }, completion: {(_) in
            toastLabel.removeFromSuperview()
        })
    }
}

extension String {
    var bool: Bool? {
        switch self.lowercased() {
        case "true", "t", "yes", "y", "1":
            return true
        case "false", "f", "no", "n", "0":
            return false
        default:
            return nil
        }
    }
}
