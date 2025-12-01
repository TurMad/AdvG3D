public enum VisitorStatus
{
    InQueue,        // появился сегодня и ждет игрока
    Served,         // игрок выбрал / взял задание / поговорил
    Left            // ушёл, не получил внимания
}