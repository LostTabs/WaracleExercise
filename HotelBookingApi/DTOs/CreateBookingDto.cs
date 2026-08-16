namespace HotelBookingApi.DTOs;

public record CreateBookingDto(
    int HotelId,
    int NumberOfGuests,
    DateOnly CheckIn,
    DateOnly CheckOut);