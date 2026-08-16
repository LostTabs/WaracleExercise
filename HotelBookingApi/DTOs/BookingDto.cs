namespace HotelBookingApi.DTOs;

public record BookingDto(
    string Reference,
    int HotelId,
    int RoomId,
    int NumberOfGuests,
    DateOnly CheckIn,
    DateOnly CheckOut);