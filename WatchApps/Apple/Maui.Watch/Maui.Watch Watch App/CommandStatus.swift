//
//  CommandStatus.swift
//  KinesiaU Watch App
//
//  Created by Jack Butler on 11/11/22.
//

import Foundation
import WatchConnectivity

public enum Command {
    case UpdateAppContext
    case SendMessage
    case SendMessageData
    case TransferFile
    case TransferUserInfo
    case TransferCurrentComplicationUserInfo
    case ManageRecording
}

public enum Phrase {
    case Updated
    case Sent
    case Received
    case Replied
    case Transferring
    case Canceled
    case Finished
    case Failed
    case Start
    case Stop
}

public struct PayloadKey {
    public static var TimeStamp: String {
        get {
            return "TimeStamp"
        }
    }
    public static var Command: String {
        get {
            return "CommandData"
        }
    }
    public static var BinaryData: String {
        get {
            return "BinaryData"
        }
    }
    public static var IsCurrentComplicationInfo: String {
        get {
            return "isCurrentComplicationInfo"
        }
    }
    public static var msgWatch: String {
        get {
            return "MessageWatch"
        }
    }
    public static var StringData: String {
        get {
            return "StringData"
        }
    }
    public static var NumberData: String {
        get {
            return "NumberData"
        }
    }
}

public class CommandData {
    private var _timeStamp: Date = Date.now
    public var timeStamp: Date {
        get { return _timeStamp }
        set { _timeStamp = newValue }
    }
    private var _command: String = ""
    public var command: String {
        get { return _command }
        set { _command = newValue }
    }
    private var _binaryData: NSArray = NSArray()
    public var binaryData: NSArray {
        get { return _binaryData }
        set { _binaryData = newValue }
    }
    private var _stringData: String = ""
    public var stringData: String {
        get { return _stringData }
        set { _stringData = newValue }
    }
    private var _numberData: Double = Double.nan
    public var numberData: Double {
        get { return _numberData }
        set { _numberData = newValue }
    }
    init() {}
    init(commandData: NSDictionary) {
        if let ts = commandData[PayloadKey.TimeStamp] as? Date
        {
            self._timeStamp = ts
        }
        if let cd = commandData[PayloadKey.Command] as? String {
            self._command = cd
        }
        if let bd = commandData[PayloadKey.BinaryData] as? NSArray {
            self._binaryData = bd
        }
        if let msg = commandData[PayloadKey.msgWatch] as? String {
            self._command = msg
        }
        if let strData = commandData[PayloadKey.StringData] as? String {
            self._stringData = strData
        }
        if let numData = commandData[PayloadKey.NumberData] as? Double {
            self._numberData = numData
        }
    }
    
    static func Create(commandData: Data) -> CommandData {
        do {
            let unarchiver = try NSKeyedUnarchiver(forReadingFrom: commandData)
            let data = try unarchiver.decodeTopLevelObject()
            if let dictionaryData = data as? NSDictionary {
                return CommandData(commandData: dictionaryData)
            }
        }
        catch {
            log(error.localizedDescription)
        }
        return CommandData()
    }
}

public class CommandStatus: NSObject {
    public var Command: Command
    public var Phrase: Phrase
    init(command: Command, phrase: Phrase, commandData: CommandData) {
        self.Command = command
        self.Phrase = phrase
        self.CommandData = commandData
    }
    
    public var CommandData: CommandData
    public var FileTransfer: WCSessionFileTransfer = WCSessionFileTransfer()
    public var File: String = ""
    public var UserInfoTransfer: WCSessionUserInfoTransfer = WCSessionUserInfoTransfer()
    public var ErrorMessage: String = ""
}
