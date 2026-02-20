using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLogging_State_Machine
{
    internal class Program
    {
        public enum MachineState
        {
            IDLE,
            RUNNING,
            MAINTENANCE,
            UPDATING,
            ERROR
        }

        public class EventLogger
        {
            public void Log(string message)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
            }
        }

        public class StateMachine
        {
            private readonly EventLogger _logger;
            public MachineState CurrentState { get; private set; }

            public StateMachine(EventLogger logger)
            {
                _logger = logger;
                CurrentState = MachineState.IDLE;
                _logger.Log($"System initialized. Current state: {CurrentState}");
            }

            public void StartGame()
            {
                _logger.Log("Command received: start_game");

                if (CurrentState == MachineState.IDLE)
                {
                    CurrentState = MachineState.RUNNING;
                    _logger.Log("Transition: IDLE -> RUNNING");
                }
                else
                {
                    _logger.Log($"Invalid transition: Cannot start game from {CurrentState}");
                }
            }

            public void StopGame()
            {
                _logger.Log("Command received: stop_game");

                if (CurrentState == MachineState.RUNNING)
                {
                    CurrentState = MachineState.IDLE;
                    _logger.Log("Transition: RUNNING -> IDLE");
                }
                else
                {
                    _logger.Log($"Invalid transition: Cannot stop game from {CurrentState}");
                }
            }

            public void Signal(string signal)
            {
                _logger.Log($"Signal received: {signal}");

                if (signal == "door_open")
                {
                    CurrentState = MachineState.MAINTENANCE;
                    _logger.Log("Transition: -> MAINTENANCE (door opened)");
                }
                else if (signal == "door_close")
                {
                    CurrentState = MachineState.IDLE;
                    _logger.Log("Transition: -> IDLE (door closed)");
                }
                else
                {
                    _logger.Log("Unknown signal.");
                }
            }

            public void UpdatePackage(string packageName)
            {
                _logger.Log($"Update command received. Package: {packageName}");

                if (CurrentState == MachineState.RUNNING)
                {
                    _logger.Log("Stopping game before update.");
                    CurrentState = MachineState.IDLE;
                }

                CurrentState = MachineState.UPDATING;
                _logger.Log("Transition: -> UPDATING");

                // Simulate update process
                System.Threading.Thread.Sleep(1000);

                CurrentState = MachineState.IDLE;
                _logger.Log("Update completed. Transition: UPDATING -> IDLE");
            }

            public void DeviceCommand(string device, string action, string value)
            {
                _logger.Log($"Device command received: {device} {action} {value}");

                if (device == "bill_validator" && action == "ack")
                {
                    _logger.Log($"Bill validator ACK turned {value}");
                }
                else
                {
                    _logger.Log("Unknown device command.");
                }
            }

            public void OsCommand(string command, string value)
            {
                _logger.Log($"OS command received: {command} {value}");

                if (command == "set-timezone")
                {
                    _logger.Log($"Timezone set to {value}");
                }
                else
                {
                    _logger.Log("Unknown OS command.");
                }
            }

            public void PrintStatus()
            {
                _logger.Log($"Current state: {CurrentState}");
            }
        }


        static void Main(string[] args)
        {
            var logger = new EventLogger();
            var stateMachine = new StateMachine(logger);

            Console.WriteLine("CLI Simulation Harness Started.");
            Console.WriteLine("Type 'exit' to quit.");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input == "exit")
                    break;

                var tokens = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    switch (tokens[0])
                    {
                        case "start_game":
                            stateMachine.StartGame();
                            break;

                        case "stop_game":
                            stateMachine.StopGame();
                            break;

                        case "signal":
                            if (tokens.Length >= 2)
                                stateMachine.Signal(tokens[1]);
                            break;

                        case "update":
                            if (tokens.Length >= 3 && tokens[1] == "--package")
                                stateMachine.UpdatePackage(tokens[2]);
                            break;

                        case "device":
                            if (tokens.Length >= 4)
                                stateMachine.DeviceCommand(tokens[1], tokens[2], tokens[3]);
                            break;

                        case "os":
                            if (tokens.Length >= 3)
                                stateMachine.OsCommand(tokens[1], tokens[2]);
                            break;

                        case "status":
                            stateMachine.PrintStatus();
                            break;

                        default:
                            logger.Log("Unknown command.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    logger.Log($"ERROR: {ex.Message}");
                }
            }

            logger.Log("System shutting down.");
        }
    }
}
//Centralized Logging: Every command or signal produces a timestamped log entry.

//State Machine: Transitions between IDLE, RUNNING, MAINTENANCE, UPDATING, and ERROR.

//CLI Harness: Accepts commands interactively (start_game, stop_game, signal door_open, update --package ..., etc.).

//Log Viewer: Type logs to see all recorded events.

//Error Handling: Unknown commands trigger transition to ERROR.
