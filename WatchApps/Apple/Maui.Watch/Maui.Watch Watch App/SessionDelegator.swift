//
//  SessionDelegator.swift
//  KinesiaU Watch App
//
//  Created by Jack Butler on 11/11/22.
//

import Foundation
import WatchConnectivity
import UniformTypeIdentifiers

/// Session Delegate to handle communication with phone. 
class SessionDelegator: NSObject, WCSessionDelegate, ObservableObject {
    private let session: WCSession
    var sentCommands: [SentCommands] = []
    
    init(session: WCSession = .default) {
        self.session = session
        super.init()
        if WCSession.isSupported() {
            self.session.delegate = self
            self.connect()
        }
    }
    
    private func checkLastSentCommand(command: String, parameter: String) -> Bool {
        return sentCommands.contains { sentCommand in
            if sentCommand.Command == command && sentCommand.Parameter == parameter && sentCommand.TimeStamp.timeIntervalSinceNow > TimeInterval(60 * 15) {
                return true
            } else {
                return false
            }
        }
    }
    
    func removeLastSentCommand(command: String, parameter: String) {
        sentCommands.removeAll { sentCommand in
            if sentCommand.Command == command && sentCommand.Parameter == parameter {
                return true
            } else {
                return false
            }
        }
    }
    
    func removeLastSentCommand(command: String) {
        sentCommands.removeAll { sentCommand in
            if sentCommand.Command == command {
                return true
            } else {
                return false
            }
        }
    }
    
    func removeSentStatusCommands() {
        sentCommands.removeAll { sentCommand in
            return sentCommand.Command == CommandStrings.Watch.Status
        }
    }
    
    /// Sends a reply back to the phone via context update
    /// - Parameters:
    ///   - reply: Command string to send to the phone.
    ///   - stringData: Extra data to send with command
    ///   - numberData: Any numerical data as a `Double`
    func sendReply(reply: String, stringData: String = "", numberData: Double = Double.nan, binaryData: NSArray = [], highPriorty: Bool = false) {
        if !checkLastSentCommand(command: reply, parameter: stringData) {
            do {
                sentCommands.append(SentCommands(Command: reply, Parameter: stringData, TimeStamp: Date.now))
                let dict: [String:Any] = [PayloadKey.TimeStamp:Date.now,
                                          PayloadKey.Command:reply,
                                          PayloadKey.BinaryData:binaryData,
                                          PayloadKey.StringData:stringData,
                                          PayloadKey.NumberData:numberData]
                if highPriorty {
                    session.transferUserInfo(dict)
                } else {
                    try session.updateApplicationContext(dict)
                }
                log("Sending CMD to Phone: \(reply)")
            } catch {
                log("Error sending command to phone: \(error.localizedDescription)")
            }
        }  else {
            log("Command \(reply) (\(stringData)) last sent less than 15 minutes ago without a response, ignoring")
        }
    }
    
    /// Determines if the phone can be reached
    ///
    /// Checks to see if the session is
    /// 1. Reachable
    /// 2. Activated
    /// 3. Has a companion app installed on the phone
    ///
    /// - Returns: If all 3 of these are `true`, then returns `true`, otherwise `false`
    func canReachPhone() -> Bool {
        //log("Session Is Reachable: \(session.isReachable); Session State: \(session.activationState)")
        return session.isReachable && session.activationState == .activated && session.isCompanionAppInstalled
    }
    
    
    /// Transfers the current log file to the phone
    func transferLogFile() {
        let fileExists = FileManager.default.fileExists(atPath: Helper.getLogPath())
        if fileExists && canReachPhone() {
            _ = transferFile(Helper.getLogFileName())
            log("Transferred Watch Log")
        }
    }
    
    /// Transfers a file from the temp directory to the phone
    /// - Parameters:
    ///  - fileName: The name of the file (with extension) to send to the phone
    /// - Returns: If the file transfer succeeds, then `true`
    func transferFile(_ fileName: String) -> Bool {
        let path = Helper.storagePath
        var fileType: UTType
        if fileName.contains(".txt") {
            fileType = .text
        } else if fileName.contains(".json") {
            fileType = .json
        } else {
            fileType = .data
        }
        let file = path.appendingPathComponent(fileName, conformingTo: fileType)
        if canReachPhone() {
            session.transferFile(file, metadata: nil)
            log("Transferring file \(file.lastPathComponent)")
            return true
        } else {
            return false
        }
    }
    
    /// Connects and activates the Watch Connectivity session
    func connect() {
        guard WCSession.isSupported() else {
            log("WCSession is not supported")
            return
        }
        
        session.activate()
    }
    
    /// Send implementation with logging
    /// - Parameters:
    ///  - message: array of strings to send to the phone
    func send(message: [String:Any]) -> Void {
        session.sendMessage(message, replyHandler: nil) {
            (error) in
            log("Error sending message to phone: " + error.localizedDescription)
        }
    }
    
    func session(_ session: WCSession, activationDidCompleteWith activationState: WCSessionActivationState, error: Error?) {
        postNotification(name: Notification.Name.ActivationDidComplete, object: nil)
    }
    
    func sessionReachabilityDidChange(_ session: WCSession) {
        postNotification(name: Notification.Name.ReachabilityDidChange, object: nil)
    }
    
    func session(_ session: WCSession, didReceiveApplicationContext applicationContext: [String : Any]) {
        let commandStatus = CommandStatus(command: Command.UpdateAppContext, phrase: Phrase.Received, commandData: CommandData(commandData: applicationContext as NSDictionary))
        log("Received passive command: \(commandStatus.CommandData.command)")
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func session(_ session: WCSession, didReceiveMessage message: [String : Any]) {
        let commandStatus = CommandStatus(command: Command.SendMessage, phrase: Phrase.Received, commandData: CommandData(commandData: message as NSDictionary))
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func session(_ session: WCSession, didReceiveMessageData messageData: Data) {
        let commandStatus = CommandStatus(command: Command.SendMessage, phrase: Phrase.Received, commandData: CommandData.Create(commandData: messageData))
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func session(_ session: WCSession, didReceiveUserInfo userInfo: [String : Any] = [:]) {
        let commandStatus = CommandStatus(command: Command.UpdateAppContext, phrase: Phrase.Received, commandData: CommandData(commandData: userInfo as NSDictionary))
        log("Received high priority command: \(commandStatus.CommandData.command)")
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func session(_ session: WCSession, didFinish userInfoTransfer: WCSessionUserInfoTransfer, error: Error?) {

    }
    
    func session(_ session: WCSession, didReceive file: WCSessionFile) {
        let commandStatus = CommandStatus(command: Command.TransferFile, phrase: Phrase.Received, commandData: CommandData())
        commandStatus.File = file.fileURL.lastPathComponent
        
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func session(_ session: WCSession, didFinish fileTransfer: WCSessionFileTransfer, error: Error?) {
        let commandStatus = CommandStatus(command: Command.TransferFile, phrase: Phrase.Finished, commandData: CommandData())
        if error != nil {
            commandStatus.ErrorMessage = error!.localizedDescription
        }
        commandStatus.FileTransfer = fileTransfer
        
        postNotification(name: Notification.Name.DataDidFlow, object: commandStatus)
    }
    
    func sessionCompanionAppInstalledDidChange(_ session: WCSession) {
        if WCSession.isSupported() {
            self.session.delegate = self
            self.connect()
        }
    }
    
    private func postNotification(name: Notification.Name, object: CommandStatus?) {
        DispatchQueue.main.async {
            NotificationCenter.default.post(name: name, object: object)
        }
    }
}


extension Notification.Name {
    static let DataDidFlow = Notification.Name("DataDidFlow")
    static let ReachabilityDidChange = Notification.Name("ReachabilityDidChange")
    static let ActivationDidComplete = Notification.Name("ActivationDidComplete")
}
