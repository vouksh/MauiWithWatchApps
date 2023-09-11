//
//  CommunicationHandler.swift
//  KinesiaU Watch App
//
//  Created by Jack Butler on 1/25/23.
//

import Foundation
import WatchConnectivity
import WatchKit

class CommunicationHandler {
    let sessionDelegate = SessionDelegator()
    var reconnectCount: Int = 0
    
    init() {
        
    }
    
    /// Sends the current status the watch/recording to the phone
    public func sendWatchStatus(_ status: AppStatus) {
        sessionDelegate.sendReply(reply:  CommandStrings.Watch.Status, stringData: "", numberData: Double(status.rawValue), highPriorty: true)
    }
    
    /// Lets the phone know that an error has occurred on the watch
    public func sendWatchError(_ errorMessage: String = "") {
        sessionDelegate.sendReply(reply: CommandStrings.Watch.Status, stringData: errorMessage, numberData: Double(AppStatus.Error.rawValue), highPriorty: true)
    }
    
    /// Processes incoming commands from the phone
    /// - Parameters:
    ///   - commandStatus: `CommandStatus` object to be passed to the phone
    public func processCommand(_ commandStatus: CommandStatus) {
        switch commandStatus.Command {
        case .UpdateAppContext,
                .SendMessage:
            var data: String = ""
            if !commandStatus.CommandData.numberData.isNaN {
                data = String(commandStatus.CommandData.numberData)
            } else if commandStatus.CommandData.binaryData.count > 1 {
                data = "Binary"
            } else {
                data = commandStatus.CommandData.stringData
            }
            log("Received context update: \(commandStatus.CommandData.command); Data: \(data)")
            processContextUpdate(commandString: commandStatus.CommandData.command, commandData: commandStatus.CommandData)
        case .TransferFile:
            log("Received file transfer: \(commandStatus.FileTransfer.file.fileURL.pathString())")
            proccessFileTransfer(commandPhrase: commandStatus.Phrase,
                                 error: commandStatus.ErrorMessage,
                                 fileURL: commandStatus.FileTransfer.file.fileURL)
        default:
            log("Unknown command received: \(commandStatus.Command)")
        }
    }
    
    /// If the command was a context update, process the command and perform the action or send a response as needed
    /// - Parameters:
    ///   - commandString: Command sent from phone. 
    private func processContextUpdate(commandString: String, commandData: CommandData) {
        switch commandString {
        case CommandStrings.Phone.QueryWatchStatus:
            reconnectCount = 0;
            sessionDelegate.removeSentStatusCommands()
            sendWatchStatus(.Running)
        case CommandStrings.Phone.TransferFiles:
            sessionDelegate.transferLogFile()
        default:
            log("Unknown command received: \(commandString)")
        }
    }
    
    /// If the command was a response to recieving a file, handle it appropriately
    private func proccessFileTransfer(commandPhrase: Phrase, error: String, fileURL: URL) {
        let fileName = fileURL.lastPathComponent
        switch commandPhrase {
        case .Finished:
            if error == "" {
                log("Phone reported file received: \(fileName)")
            } else {
                log("Error while transferring file to phone: \(error)")
                Task {
                    if #available(watchOS 9.0, *) {
                        try await Task.sleep(for: Duration.seconds(5))
                    } else {
                        try await Task.sleep(nanoseconds: 5000000000)
                    }
                    log("Retrying transfer of file \(fileName)")
                    _ = sessionDelegate.transferFile(fileName)
                }
            }
        case .Canceled:
            log("Phone reported file transfer cancelled for file \(fileName)")
        case .Failed:
            log("Phone reported file transfer failed: \(fileName)")
            log("Error reported: \(error)")
        case .Sent:
            log("File sent to phone: \(fileName)")
        default:
            log("Unhandled file transfer phrase: \(commandPhrase)")
        }
    }
}

struct CommandStrings {
    struct Phone {
        static let QueryWatchStatus: String = "QUERY_STATUS"
        static let TransferFiles: String = "TRANSFER_FILES"
    }
    struct Watch {
        static let Status: String = "WATCH_STATUS"
    }
}

enum AppStatus: Int {
    case Unknown
    case Running
    case Stopped
    case Error
    case Syncing
}

struct SentCommands {
    var Command: String = ""
    var Parameter: String = ""
    var TimeStamp: Date = Date()
}

